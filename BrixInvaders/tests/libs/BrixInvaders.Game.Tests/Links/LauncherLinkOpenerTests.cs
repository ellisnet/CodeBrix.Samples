using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrixInvaders.Assets;
using BrixInvaders.Game.Links;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Links;

//The launcher opener without a UI: the "UI thread" is a list the test runs, and the launcher is a fake.
public class LauncherLinkOpenerTests
{
    private readonly List<Action> _uiQueue = new List<Action>();
    private readonly List<Uri> _launched = new List<Uri>();

    private bool Post(Action action)
    {
        _uiQueue.Add(action);
        return true;
    }

    private void RunUiThread()
    {
        var queued = _uiQueue.ToArray();
        _uiQueue.Clear();
        foreach (var action in queued)
        {
            action();
        }
    }

    private Func<Uri, Task<bool>> Launcher(bool result) => uri =>
    {
        _launched.Add(uri);
        return Task.FromResult(result);
    };

    [Fact]
    public async Task Open_launches_on_the_UI_thread_and_answers_true_when_a_browser_took_the_link()
    {
        //Arrange
        var opener = new LauncherLinkOpener(Post, Launcher(true));

        //Act
        var answer = opener.Open(KenneyPacks.BundleUrl);
        var launchedBeforeUiRan = _launched.Count;
        RunUiThread();
        var opened = await answer.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        //Assert
        launchedBeforeUiRan.Should().Be(0, "the launcher runs on the UI thread, never the engine thread");
        opened.Should().BeTrue();
        _launched.Should().Equal(new Uri(KenneyPacks.BundleUrl));
    }

    [Fact]
    public async Task Open_answers_false_when_no_browser_was_available()
    {
        //Arrange
        var opener = new LauncherLinkOpener(Post, Launcher(false));

        //Act
        var answer = opener.Open(KenneyPacks.BundleUrl);
        RunUiThread();
        var opened = await answer.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        //Assert
        opened.Should().BeFalse();
    }

    [Fact]
    public async Task Open_answers_false_when_the_launcher_throws()
    {
        //Arrange
        var opener = new LauncherLinkOpener(Post, _ => throw new InvalidOperationException("no launcher"));

        //Act
        var answer = opener.Open("https://kenney.nl");
        RunUiThread();
        var opened = await answer.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        //Assert
        opened.Should().BeFalse();
    }

    [Fact]
    public async Task Open_answers_false_when_the_UI_thread_cannot_take_the_work()
    {
        //Arrange
        var opener = new LauncherLinkOpener(_ => false, Launcher(true));

        //Act
        var opened = await opener.Open(KenneyPacks.BundleUrl).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        //Assert
        opened.Should().BeFalse();
        _launched.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a link")]
    [InlineData("file:///etc/passwd")]
    public async Task Open_refuses_anything_but_a_web_address(string url)
    {
        //Arrange
        var opener = new LauncherLinkOpener(Post, Launcher(true));

        //Act
        var opened = await opener.Open(url).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        //Assert
        opened.Should().BeFalse();
        _uiQueue.Should().BeEmpty();
        _launched.Should().BeEmpty();
    }

    [Fact]
    public void the_constructor_needs_a_way_to_reach_the_UI_thread() =>
        ((Action)(() => _ = new LauncherLinkOpener(null))).Should().Throw<ArgumentNullException>();
}
