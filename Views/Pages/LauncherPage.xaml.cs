using System.Windows.Controls;
using System.Windows.Input;
using PhoebeEditor.Models;
using PhoebeEditor.ViewModels;

namespace PhoebeEditor.Views.Pages;

public partial class LauncherPage : Page
{
    private MainViewModel ViewModel { get; }

    public LauncherPage(MainViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
    }

    private void SteamToggle_Click(object sender, MouseButtonEventArgs e)
    {
        if (ViewModel.LauncherType != LauncherType.Steam)
            ViewModel.LauncherType = LauncherType.Steam;
    }

    private void KuroToggle_Click(object sender, MouseButtonEventArgs e)
    {
        if (ViewModel.LauncherType != LauncherType.Kuro)
            ViewModel.LauncherType = LauncherType.Kuro;
    }
}