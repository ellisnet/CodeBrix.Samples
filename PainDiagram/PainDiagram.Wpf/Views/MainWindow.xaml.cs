using CodeBrix.Imaging.Drawing;
using PainDiagram.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace PainDiagram.Views;

public partial class MainWindow : Window
{
    //I tend to like to declare/define private methods above the constructor, in C# classes
    private MainViewModel ViewModel => DataContext as MainViewModel;

    public MainWindow()
    {
        //Doing this before InitializeComponent() - in case InitializeComponent()
        //  is the thing that sets the data context.
        DataContextChanged += (_, _) =>
        {
            if (DataContext is IFileSaveBridge fileSave)
            {
                fileSave.PickSavePngPathAsync = PickSavePngPathAsync;
            }

            if (DataContext is ICanvasInvalidator invalidator)
            {
                invalidator.InvalidateCanvas = InvalidateDrawCanvas;
            }
        };

        InitializeComponent();

        //Paint, press, move, release and capture-lost all go straight to the drawing session
        DrawCanvas.BindToSession(() => ViewModel?.Session);
    }

    private void InvalidateDrawCanvas()
    {
        if (DrawCanvas.Dispatcher.CheckAccess())
        {
            DrawCanvas.InvalidateVisual();
        }
        else
        {
            DrawCanvas.Dispatcher.BeginInvoke(DrawCanvas.InvalidateVisual);
        }
    }

    private Task<string> PickSavePngPathAsync(string suggestedFileName)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Save PNG as",
            Filter = "PNG image (*.png)|*.png|All files (*.*)|*.*",
            DefaultExt = ".png",
            AddExtension = true,
            FileName = suggestedFileName,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            OverwritePrompt = false   //The app does its own replace prompt via SimpleDialog
        };

        var chosen = dialog.ShowDialog(this) == true ? dialog.FileName : null;
        return Task.FromResult(chosen);
    }
}
