using System.Windows.Controls;
using PhoebeEditor.ViewModels;

namespace PhoebeEditor.Views.Pages;

public partial class LauncherPage : Page
{
    public LauncherPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
