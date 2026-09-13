using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WikipediaPublisher.RenderArticle.Tests;

/// <summary>
/// Builds the container the application builds, by calling the pipeline library's own
/// AddRenderArticle() registration method, so a test can resolve IArticleRenderService the
/// same way the view model does instead of constructing the implementation itself.
/// </summary>
public class RenderArticleTestingFixture : SimpleTestFixture
{
    protected override void RegisterCustomServices(
        IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration config,
        Func<IServiceProvider> serviceResolver)
    {
        //The application registers the pipeline exactly this way, inside the
        //  SimpleServiceResolver.CreateInstance() callback in its App constructor
        services.AddRenderArticle();
    }
}
