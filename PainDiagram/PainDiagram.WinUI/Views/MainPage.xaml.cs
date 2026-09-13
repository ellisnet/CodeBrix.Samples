using CodeBrix.Imaging.Drawing;
using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml.Controls;
using PainDiagram.ViewModels;
using System.Threading.Tasks;

namespace PainDiagram.WinUI.Views;

// ReSharper disable once RedundantExtendsListEntry
public sealed partial class MainPage : Page
{
    //I tend to like to declare/define private methods above the constructor, in C# classes
    private MainViewModel ViewModel => DataContext as MainViewModel;

    public MainPage()
    {
        //Doing this before InitializeComponent() - in case InitializeComponent()
        //  is the thing that sets the data context.
        DataContextChanged += (_, _) =>
        {
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

            if (DataContext is IFileSaveBridge fileSave)
            {
                fileSave.PickSavePngPathAsync = (fileName) =>
                {
                    //The Win32 dialog (rather than the WinRT FileSavePicker) so the un-suppressible
                    //  WinRT overwrite prompt does not double up with the app's own confirmation
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.CurrentWindow);
                    var path = Win32SaveFileDialog.PickSavePath(hwnd, fileName, "Save PNG as");
                    return Task.FromResult(path);
                };
            }

            if (DataContext is ICanvasInvalidator invalidator)
            {
                invalidator.InvalidateCanvas = () => DrawCanvas?.Invalidate();
            }
        };

        InitializeComponent();

        //Paint, press, move, release and capture-lost all go straight to the drawing session
        DrawCanvas.BindToSession(() => ViewModel?.Session);
    }
}
