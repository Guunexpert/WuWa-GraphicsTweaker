using System.Windows;
using System.Windows.Controls;
using PhoebeEditor.ViewModels;

namespace PhoebeEditor.Views.Pages;

public partial class PresetsPage : Page
{
    private readonly MainViewModel _viewModel;

    public PresetsPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private void SavePreset_Click(object sender, RoutedEventArgs e)
    {
        var name = TxtPresetName.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            TxtPresetName.Focus();
            return;
        }

        _viewModel.SavePresetCommand.Execute(name);
        TxtPresetName.Text = string.Empty;
    }
}
