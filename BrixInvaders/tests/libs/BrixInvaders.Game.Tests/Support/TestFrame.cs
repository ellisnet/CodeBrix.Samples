using System;
using System.Collections.Generic;
using System.Linq;
using BrixInvaders.Game.Rendering;
using CodeBrix.Platform.GameEngine.Drawing.Direct.DrawLists;
using SkiaSharp;

namespace BrixInvaders.Game.Tests.Support;

/// <summary>
/// The game's two draw lists over a picture library in which every catalog key is a tiny picture of its own, so a
/// test can read back which key each image command drew. Text uses the default typeface (nothing is rendered).
/// </summary>
internal sealed class TestFrame
{
    private static readonly string[] Keys = SpriteCatalog.AllAtlasFrames.Concat(SpriteCatalog.LoosePictures)
        .Append(SpriteCatalog.PromoCard).Distinct(StringComparer.Ordinal).ToArray();

    private static readonly SKImage[] Pictures = Keys.Select(_ => NewPicture()).ToArray();

    private static readonly Dictionary<SKImage, string> KeyByPicture =
        Keys.Select((key, index) => (key, index))
            .ToDictionary<(string key, int index), SKImage, string>(pair => Pictures[pair.index], pair => pair.key, ReferenceEqualityComparer.Instance);

    /// <summary>Creates empty lists over a fresh library.</summary>
    public TestFrame()
    {
        for (var i = 0; i < Keys.Length; i++)
        {
            Images.AddImage(Keys[i], null, Pictures[i]);
        }

        Lists = new FrameLists(new DrawList(Images), new DrawList(Images), SKTypeface.Default, SKTypeface.Default);
    }

    /// <summary>The picture library (its <c>Missing</c> lists any key a painter drew that the catalog does not know).</summary>
    public DrawImageLibrary Images { get; } = new DrawImageLibrary();

    /// <summary>The lists the painters fill.</summary>
    public FrameLists Lists { get; }

    /// <summary>The world commands built so far.</summary>
    public IReadOnlyList<Drawn> World => Read(Lists.World);

    /// <summary>The overlay commands built so far.</summary>
    public IReadOnlyList<Drawn> Overlay => Read(Lists.Overlay);

    /// <summary>The links (hit region ids) on the overlay.</summary>
    public IEnumerable<string> Links => Lists.Overlay.HitRegions.Select(region => region.Id);

    private static IReadOnlyList<Drawn> Read(DrawList list) =>
        list.Commands.Select(command =>
            new Drawn(command.Kind, command.Image == null ? null : KeyByPicture[command.Image], command.Text)).ToList();

    private static SKImage NewPicture()
    {
        using var bitmap = new SKBitmap(1, 1);
        return SKImage.FromBitmap(bitmap);
    }

    /// <summary>One command as a test reads it.</summary>
    /// <param name="Kind">What it draws.</param>
    /// <param name="Image">The catalog key of the picture (image commands).</param>
    /// <param name="Text">The text (text commands).</param>
    public sealed record Drawn(DrawCommandKind Kind, string Image, string Text);
}
