using System;
using SilverAssertions;
using WikipediaPublisher.RenderArticle.Services;
using Xunit;

namespace WikipediaPublisher.RenderArticle.Tests;

public class RegisterServicesTests : IClassFixture<RenderArticleTestingFixture>
{
    private readonly RenderArticleTestingFixture _fixture;

    public RegisterServicesTests(RenderArticleTestingFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    [Fact]
    public void AddRenderArticle_registers_the_article_render_service()
    {
        //Arrange - the fixture built its container by calling AddRenderArticle()

        //Act
        var service = _fixture.GetService<IArticleRenderService>();

        //Assert
        service.Should().NotBeNull();
        service.Should().BeOfType<ArticleRenderService>();
    }

    [Fact]
    public void AddRenderArticle_registers_the_article_render_service_as_a_singleton()
    {
        //Arrange - the registration is AddSingleton, so every caller gets one instance

        //Act
        var first = _fixture.GetService<IArticleRenderService>();
        var second = _fixture.GetService<IArticleRenderService>();

        //Assert
        second.Should().BeSameAs(first);
    }
}
