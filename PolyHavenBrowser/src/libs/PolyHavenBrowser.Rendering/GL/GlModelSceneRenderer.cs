using System.Numerics;
using CodeBrix.Platform.OpenGL;

namespace PolyHavenBrowser.Rendering;

/// <summary>
/// The default <see cref="IModelSceneRenderer"/>: uploads a <see cref="LoadedModel"/> to
/// vertex buffers and textures, and draws it with simple headlight shading (ambient +
/// N·L from the camera direction) modulated by the material base color and texture.
/// Works against both desktop OpenGL 3.3 and OpenGL ES 3.0 contexts — the full set the
/// six CodeBrix.Platform heads provide (WGL/GLX give desktop GL; ANGLE, Wayland EGL, and
/// the FrameBuffer wrapper give GLES).
/// </summary>
public sealed class GlModelSceneRenderer : IModelSceneRenderer
{
    private const string VertexShaderBody = """
        layout(location = 0) in vec3 aPosition;
        layout(location = 1) in vec3 aNormal;
        layout(location = 2) in vec2 aTexCoord;
        uniform mat4 uMvp;
        out vec3 vNormal;
        out vec2 vTexCoord;
        void main()
        {
            gl_Position = uMvp * vec4(aPosition, 1.0);
            vNormal = aNormal;
            vTexCoord = aTexCoord;
        }
        """;

    private const string FragmentShaderBody = """
        precision mediump float;
        in vec3 vNormal;
        in vec2 vTexCoord;
        uniform vec4 uBaseColorFactor;
        uniform sampler2D uBaseColorTexture;
        uniform int uHasTexture;
        uniform vec3 uLightDirection;
        uniform int uDoubleSided;
        out vec4 fragColor;
        void main()
        {
            vec3 normal = normalize(vNormal);
            float nDotL = dot(normal, normalize(uLightDirection));
            // Double-sided (abs) lights back faces too, which reads correctly for a headlit
            // model or double-sided foliage; single-sided gives a solid shape real form.
            float diffuse = (uDoubleSided == 1) ? abs(nDotL) : max(nDotL, 0.0);
            float light = 0.25 + 0.75 * diffuse;
            vec4 baseColor = uBaseColorFactor;
            if (uHasTexture == 1)
            {
                baseColor *= texture(uBaseColorTexture, vTexCoord);
            }
            fragColor = vec4(baseColor.rgb * light, baseColor.a);
        }
        """;

    private sealed record GpuPrimitive(
        uint Vao, uint PositionBuffer, uint NormalBuffer, uint TexCoordBuffer, uint IndexBuffer,
        uint IndexCount, int MaterialIndex);

    private readonly object _pendingLock = new();

    private LoadedModel? _pendingModel;
    private bool _pendingFrameCamera;
    private bool _hasPendingModel;

    private LoadedModel? _currentModel;
    private readonly List<GpuPrimitive> _gpuPrimitives = [];
    private readonly Dictionary<int, uint> _materialTextures = [];

    private uint _program;
    private int _mvpLocation;
    private int _baseColorFactorLocation;
    private int _hasTextureLocation;
    private int _lightDirectionLocation;
    private int _doubleSidedLocation;
    private int _baseColorTextureLocation;
    private bool _initialized;

    /// <inheritdoc />
    public OrbitCamera Camera { get; } = new();

    /// <inheritdoc />
    public (float R, float G, float B, float A) BackgroundColor { get; set; } = (0.13f, 0.13f, 0.15f, 1f);

    /// <summary>
    /// A fixed world-space light direction (pointing toward the light) that shades faces
    /// by orientation, giving a solid shape clear form. When <see langword="null"/> (the
    /// default) a camera headlight is used, which double-sides the lighting — better for
    /// flat or foliage models but ambiguous on a symmetric solid such as a cube.
    /// </summary>
    public Vector3? FixedLightDirection { get; set; }

    /// <inheritdoc />
    public void SetModel(LoadedModel? model, bool frameCamera = true)
    {
        lock (_pendingLock)
        {
            _pendingModel = model;
            _pendingFrameCamera = frameCamera;
            _hasPendingModel = true;
        }
    }

    /// <inheritdoc />
    public void Initialize(GL gl)
    {
        ArgumentNullException.ThrowIfNull(gl);

        var isGles = (gl.GetStringS(StringName.Version) ?? string.Empty)
            .Contains("OpenGL ES", StringComparison.OrdinalIgnoreCase);
        var header = isGles ? "#version 300 es\n" : "#version 330 core\n";

        var vertexShader = CompileShader(gl, ShaderType.VertexShader, header + VertexShaderBody);
        var fragmentShader = CompileShader(gl, ShaderType.FragmentShader, header + FragmentShaderBody);

        _program = gl.CreateProgram();
        gl.AttachShader(_program, vertexShader);
        gl.AttachShader(_program, fragmentShader);
        gl.LinkProgram(_program);
        gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out var linked);
        gl.DetachShader(_program, vertexShader);
        gl.DetachShader(_program, fragmentShader);
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
        if (linked == 0)
        {
            var log = gl.GetProgramInfoLog(_program);
            gl.DeleteProgram(_program);
            _program = 0;
            throw new InvalidOperationException($"Shader program link failed: {log}");
        }

        _mvpLocation = gl.GetUniformLocation(_program, "uMvp");
        _baseColorFactorLocation = gl.GetUniformLocation(_program, "uBaseColorFactor");
        _hasTextureLocation = gl.GetUniformLocation(_program, "uHasTexture");
        _lightDirectionLocation = gl.GetUniformLocation(_program, "uLightDirection");
        _doubleSidedLocation = gl.GetUniformLocation(_program, "uDoubleSided");
        _baseColorTextureLocation = gl.GetUniformLocation(_program, "uBaseColorTexture");
        _initialized = true;
    }

    /// <inheritdoc />
    public unsafe void Render(GL gl, uint width, uint height)
    {
        ArgumentNullException.ThrowIfNull(gl);
        if (!_initialized || width == 0 || height == 0)
        {
            return;
        }

        ApplyPendingModel(gl);

        gl.Viewport(0, 0, width, height);
        var (r, g, b, a) = BackgroundColor;
        gl.ClearColor(r, g, b, a);
        gl.Enable(EnableCap.DepthTest);
        gl.Disable(EnableCap.CullFace); // Poly Haven models frequently rely on double-sided rendering
        gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        if (_currentModel is null || _gpuPrimitives.Count == 0)
        {
            return;
        }

        gl.UseProgram(_program);

        // Model transforms are baked at load time, so MVP = view * projection. System.Numerics
        // matrices are row-major; transpose for GL's column-major uniform layout.
        var viewProjection = Camera.GetViewMatrix() * Camera.GetProjectionMatrix(width / (float)height);
        var mvp = Matrix4x4.Transpose(viewProjection);
        gl.UniformMatrix4(_mvpLocation, 1, false, (float*)&mvp);

        // A fixed light shades a solid shape by orientation; otherwise a headlight (from the
        // eye toward the target) with double-sided lighting suits flat and foliage models.
        var lightDirection = FixedLightDirection is { } fixedLight && fixedLight != Vector3.Zero
            ? Vector3.Normalize(fixedLight)
            : Vector3.Normalize(Camera.GetEyePosition() - Camera.Target);
        gl.Uniform3(_lightDirectionLocation, lightDirection.X, lightDirection.Y, lightDirection.Z);
        gl.Uniform1(_doubleSidedLocation, FixedLightDirection is null ? 1 : 0);
        gl.Uniform1(_baseColorTextureLocation, 0);

        foreach (var primitive in _gpuPrimitives)
        {
            var material = primitive.MaterialIndex >= 0 && primitive.MaterialIndex < _currentModel.Materials.Count
                ? _currentModel.Materials[primitive.MaterialIndex]
                : null;
            var baseColor = material?.BaseColorFactor ?? Vector4.One;
            gl.Uniform4(_baseColorFactorLocation, baseColor.X, baseColor.Y, baseColor.Z, baseColor.W);

            var hasTexture = _materialTextures.TryGetValue(primitive.MaterialIndex, out var texture);
            gl.Uniform1(_hasTextureLocation, hasTexture ? 1 : 0);
            if (hasTexture)
            {
                gl.ActiveTexture(TextureUnit.Texture0);
                gl.BindTexture(TextureTarget.Texture2D, texture);
            }

            gl.BindVertexArray(primitive.Vao);
            gl.DrawElements(PrimitiveType.Triangles, primitive.IndexCount, DrawElementsType.UnsignedInt, (void*)0);
        }

        gl.BindVertexArray(0);
        gl.UseProgram(0);
    }

    /// <inheritdoc />
    public void Uninitialize(GL gl)
    {
        ArgumentNullException.ThrowIfNull(gl);
        ReleaseModelResources(gl);
        if (_program != 0)
        {
            gl.DeleteProgram(_program);
            _program = 0;
        }
        _initialized = false;
    }

    private void ApplyPendingModel(GL gl)
    {
        LoadedModel? model;
        bool frameCamera;
        lock (_pendingLock)
        {
            if (!_hasPendingModel)
            {
                return;
            }
            model = _pendingModel;
            frameCamera = _pendingFrameCamera;
            _pendingModel = null;
            _hasPendingModel = false;
        }

        ReleaseModelResources(gl);
        _currentModel = model;
        if (model is null)
        {
            return;
        }

        foreach (var primitive in model.Primitives)
        {
            _gpuPrimitives.Add(UploadPrimitive(gl, primitive));
        }

        for (var materialIndex = 0; materialIndex < model.Materials.Count; materialIndex++)
        {
            var material = model.Materials[materialIndex];
            if (material.BaseColorTextureRgba is { Length: > 0 } rgba)
            {
                _materialTextures[materialIndex] = UploadTexture(
                    gl, rgba, (uint)material.BaseColorTextureWidth, (uint)material.BaseColorTextureHeight);
            }
        }

        if (frameCamera)
        {
            Camera.FitToModel(model);
        }
    }

    private static unsafe GpuPrimitive UploadPrimitive(GL gl, ModelPrimitive primitive)
    {
        var vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        var positionBuffer = UploadAttribute(gl, 0, 3, primitive.Positions);
        var normalBuffer = UploadAttribute(gl, 1, 3, primitive.Normals);
        var texCoordBuffer = UploadAttribute(gl, 2, 2, primitive.TexCoords);

        var indexBuffer = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, indexBuffer);
        gl.BufferData<uint>(BufferTargetARB.ElementArrayBuffer, primitive.Indices, BufferUsageARB.StaticDraw);

        gl.BindVertexArray(0);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);

        return new GpuPrimitive(
            vao, positionBuffer, normalBuffer, texCoordBuffer, indexBuffer,
            (uint)primitive.Indices.Length, primitive.MaterialIndex);
    }

    private static unsafe uint UploadAttribute(GL gl, uint location, int components, float[] data)
    {
        var buffer = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, buffer);
        gl.BufferData<float>(BufferTargetARB.ArrayBuffer, data, BufferUsageARB.StaticDraw);
        gl.EnableVertexAttribArray(location);
        gl.VertexAttribPointer(location, components, VertexAttribPointerType.Float, false, 0, (void*)0);
        return buffer;
    }

    private static uint UploadTexture(GL gl, byte[] rgba, uint width, uint height)
    {
        var texture = gl.GenTexture();
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, texture);
        gl.TexImage2D<byte>(
            TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba8, width, height, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, rgba);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
        gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);
        gl.GenerateMipmap(TextureTarget.Texture2D);
        gl.BindTexture(TextureTarget.Texture2D, 0);
        return texture;
    }

    private void ReleaseModelResources(GL gl)
    {
        foreach (var primitive in _gpuPrimitives)
        {
            gl.DeleteBuffer(primitive.PositionBuffer);
            gl.DeleteBuffer(primitive.NormalBuffer);
            gl.DeleteBuffer(primitive.TexCoordBuffer);
            gl.DeleteBuffer(primitive.IndexBuffer);
            gl.DeleteVertexArray(primitive.Vao);
        }
        _gpuPrimitives.Clear();

        foreach (var texture in _materialTextures.Values)
        {
            gl.DeleteTexture(texture);
        }
        _materialTextures.Clear();

        _currentModel = null;
    }

    private static uint CompileShader(GL gl, ShaderType type, string source)
    {
        var shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);
        gl.GetShader(shader, ShaderParameterName.CompileStatus, out var compiled);
        if (compiled == 0)
        {
            var log = gl.GetShaderInfoLog(shader);
            gl.DeleteShader(shader);
            throw new InvalidOperationException($"{type} compilation failed: {log}");
        }

        return shader;
    }
}
