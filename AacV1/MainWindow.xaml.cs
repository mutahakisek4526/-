using System.Windows;
using System.Windows.Input;
using AacV1.VM;

namespace AacV1;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        PreviewKeyDown += HandlePreviewKeyDown;
    }

    private void HandlePreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
        {
            return;
        }

        viewModel.InputService.HandleKeyDown(e.Key);
    }
}
