using CodeBrix.Platform.Simple;
using System.Windows;
using WikipediaPublisher.Helpers;
using WikipediaPublisher.RenderArticle;

namespace WikipediaPublisher;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        SimpleServiceResolver.CreateInstance(HostHelper.GetHost(), services =>
        {
            //Register my custom services here
            services.AddRenderArticle();
        });
        SimpleViewModel.SetIsDesignMode(false);
    }
}
