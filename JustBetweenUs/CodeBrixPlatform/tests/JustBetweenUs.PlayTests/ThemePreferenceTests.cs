using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI.Xaml;
using SilverAssertions;
using Xunit;

namespace JustBetweenUs.PlayTests;

// The assembly is serialized; environment changes are always restored before another test runs.
public sealed class ThemePreferenceTests
{
    [Theory]
    [InlineData(null, null, ApplicationTheme.Light)]
    [InlineData("", "Dark", ApplicationTheme.Dark)]
    [InlineData(null, "", ApplicationTheme.Light)]
    [InlineData(null, "Light", ApplicationTheme.Light)]
    [InlineData(null, "dArK", ApplicationTheme.Dark)]
    [InlineData("dark", "Light", ApplicationTheme.Dark)]
    [InlineData("light", "Dark", ApplicationTheme.Light)]
    [InlineData("DARK", "invalid", ApplicationTheme.Dark)]
    [InlineData("LiGhT", null, ApplicationTheme.Light)]
    public void Theme_preference_order_is_environment_project_light(
        string environment, string project, ApplicationTheme expected)
    {
        WithEnvironment(environment, () => new PlayTestOptions { ConfigurationAssembly = Configuration(project) }
            .ResolveTheme().Should().Be(expected));
    }

    [Theory]
    [InlineData("system", "Dark", "CODEBRIX_PLAYTEST_THEME")]
    [InlineData("1", "Light", "CODEBRIX_PLAYTEST_THEME")]
    [InlineData(" ", null, "CODEBRIX_PLAYTEST_THEME")]
    [InlineData(null, "auto", "CodeBrixPlayTestPreferredTheme")]
    [InlineData(null, "0", "CodeBrixPlayTestPreferredTheme")]
    public void Invalid_theme_preferences_identify_the_setting(string environment, string project, string source)
    {
        WithEnvironment(environment, () =>
        {
            Action resolve = () => new PlayTestOptions { ConfigurationAssembly = Configuration(project) }.ResolveTheme();
            resolve.Should().Throw<ArgumentException>().WithMessage($"*{source} must be Light or Dark*");
        });
    }

    [Fact]
    public void Nuget_build_target_embeds_the_project_theme()
    {
        var assembly = typeof(AppFixture).Assembly;
        var attributes = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Where(a => a.Key == "CodeBrixPlayTestPreferredTheme").ToArray();
        attributes.Should().ContainSingle();
        var expected = Enum.Parse<ApplicationTheme>(attributes[0].Value, ignoreCase: true);
        WithEnvironment(null, () => new PlayTestOptions { ConfigurationAssembly = assembly }
            .ResolveTheme().Should().Be(expected));
    }

    private static Assembly Configuration(string preference)
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Theme_" + Guid.NewGuid().ToString("N")),
            AssemblyBuilderAccess.RunAndCollect);
        if (preference != null)
            assembly.SetCustomAttribute(new CustomAttributeBuilder(
                typeof(AssemblyMetadataAttribute).GetConstructor(new[] { typeof(string), typeof(string) }),
                new object[] { "CodeBrixPlayTestPreferredTheme", preference }));
        return assembly;
    }

    private static void WithEnvironment(string value, Action action)
    {
        const string name = "CODEBRIX_PLAYTEST_THEME";
        var original = Environment.GetEnvironmentVariable(name);
        try { Environment.SetEnvironmentVariable(name, value); action(); }
        finally { Environment.SetEnvironmentVariable(name, original); }
    }
}
