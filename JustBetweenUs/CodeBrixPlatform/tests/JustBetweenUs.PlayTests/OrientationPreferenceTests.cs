using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace JustBetweenUs.PlayTests;

// The assembly is serialized, so temporary environment overrides cannot race application tests.
public sealed class OrientationPreferenceTests
{
    [Theory]
    [InlineData(null, null, null, ScreenOrientation.Landscape)]
    [InlineData(null, "", "Portrait", ScreenOrientation.Portrait)]
    [InlineData(null, null, "Landscape", ScreenOrientation.Landscape)]
    [InlineData(null, "portrait", "Landscape", ScreenOrientation.Portrait)]
    [InlineData(null, "landscape", "Portrait", ScreenOrientation.Landscape)]
    [InlineData(ScreenOrientation.Landscape, "portrait", "Portrait", ScreenOrientation.Landscape)]
    [InlineData(ScreenOrientation.Portrait, "landscape", "Landscape", ScreenOrientation.Portrait)]
    [InlineData(ScreenOrientation.Landscape, "invalid", "invalid", ScreenOrientation.Landscape)]
    [InlineData(null, "PORTRAIT", "invalid", ScreenOrientation.Portrait)]
    public void Preference_order_is_code_environment_project_landscape(
        ScreenOrientation? code, string environment, string project, ScreenOrientation expected)
    {
        WithEnvironment(environment, () =>
        {
            var options = new PlayTestOptions { ConfigurationAssembly = Configuration(project) };
            if (code.HasValue) options.Orientation = code.Value;
            options.ResolveOrientation().Should().Be(expected);
        });
    }

    [Theory]
    [InlineData("sideways", null)]
    [InlineData("1", "Portrait")]
    [InlineData(null, "sideways")]
    [InlineData(null, "0")]
    public void Invalid_selected_preferences_fail_instead_of_silently_using_landscape(string environment, string project)
    {
        WithEnvironment(environment, () =>
        {
            Action resolve = () => new PlayTestOptions { ConfigurationAssembly = Configuration(project) }.ResolveOrientation();
            resolve.Should().Throw<ArgumentException>();
        });
    }

    [Fact]
    public void Nuget_build_target_embeds_the_project_preference()
    {
        var assembly = typeof(AppFixture).Assembly;
        var attributes = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Where(a => a.Key == "CodeBrixPlayTestPreferredOrientation").ToArray();
        attributes.Should().ContainSingle();
        var expected = Enum.Parse<ScreenOrientation>(attributes[0].Value, ignoreCase: true);
        WithEnvironment(null, () => new PlayTestOptions { ConfigurationAssembly = assembly }
            .ResolveOrientation().Should().Be(expected));
    }

    [Fact]
    public void Conflicting_or_invalid_case_requirements_fail()
    {
        var method = typeof(OrientationPreferenceTests).GetMethod(nameof(Conflicting_or_invalid_case_requirements_fail));
        Action conflicting = () => PlayTestOrientationAttribute.Resolve(method, new[] { "Landscape", "Portrait" });
        conflicting.Should().Throw<ArgumentException>().WithMessage("*both Landscape and Portrait*");
        Action invalid = () => PlayTestOrientationAttribute.Resolve(method, new[] { "1" });
        invalid.Should().Throw<ArgumentException>();
    }

    private static Assembly Configuration(string preference)
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Orientation_" + Guid.NewGuid().ToString("N")),
            AssemblyBuilderAccess.RunAndCollect);
        if (preference != null)
            assembly.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(AssemblyMetadataAttribute).GetConstructor(new[] { typeof(string), typeof(string) }),
                new object[] { "CodeBrixPlayTestPreferredOrientation", preference }));
        return assembly;
    }

    private static void WithEnvironment(string value, Action action)
    {
        const string name = "CODEBRIX_PLAYTEST_ORIENTATION";
        var original = Environment.GetEnvironmentVariable(name);
        const string commandLine = "CodeBrix.Platform.PlayTest.CommandLineOrientation";
        var originalCommandLine = AppContext.GetData(commandLine);
        try { AppContext.SetData(commandLine, null); Environment.SetEnvironmentVariable(name, value); action(); }
        finally { AppContext.SetData(commandLine, originalCommandLine); Environment.SetEnvironmentVariable(name, original); }
    }
}
