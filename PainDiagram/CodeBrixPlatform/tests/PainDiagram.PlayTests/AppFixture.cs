using CodeBrix.Imaging.Drawing;
using CodeBrix.Samples.PlayTests;
using Microsoft.UI.Xaml;
using PainDiagram.ViewModels;
using PainDiagram.Views;

namespace PainDiagram.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    public MainViewModel Model => (MainViewModel)View.DataContext;
    public DrawingSession Session => Model.Session;
    public DrawingCanvas Canvas => (DrawingCanvas)View.FindName("DrawCanvas");
    protected override Application CreateApplication() => new App();
}
