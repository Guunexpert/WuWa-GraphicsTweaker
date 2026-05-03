using System.Windows.Controls;
using PhoebeEditor.ViewModels;

namespace PhoebeEditor.Views.Pages;

public partial class TweaksPage : Page
{
    public TweaksPage(TweaksViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
