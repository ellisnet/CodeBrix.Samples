using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrixInvaders.Assets;
using BrixInvaders.Game.Links;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Links;

//The opener without a UI: the engine's link helper is replaced by a fake that records what it was asked to open.
public class LauncherLinkOpenerTests
{
    private readonly List<Uri> _opened = new List<Uri>();

    private Func<Uri, Task<bool>> Launcher(bool result) => uri =>
    {
        _opened.Add(uri);
        return Task.FromResult(result);
    };

    [Fact]
    public async Task Open_answers_true_when_a_browser_took_the_link()
    {
        //Arrange
        var opener = new LauncherLinkOpener(Launcher(true));

        //Act
        var opened = await opener.Open(KenneyPacks.BundleUrl).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        //Assert
        opened.Should().BeTrue();
        _opened.Should().Equal(new Uri(KenneyPacks.BundleUrl));
    }

    [Fact]
    public async Task Open_answers_false_when_no_browser_was_available() =>
        (await new LauncherLinkOpener(Launcher(false)).Open(KenneyPacks.BundleUrl)
            .WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken)).Should().BeFalse();

    [Theory]
    [InlineData("")]
    [InlineData("not a link")]
    [InlineData("file:///etc/passwd")]
    public async Task Open_refuses_anything_but_a_web_address(string url)
    {
        //Arrange
        var opener = new LauncherLinkOpener(Launcher(true));

        //Act
        var opened = await opener.Open(url).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        //Assert
        opened.Should().BeFalse();
        _opened.Should().BeEmpty();
    }
}
