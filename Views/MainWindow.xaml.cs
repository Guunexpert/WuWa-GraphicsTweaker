using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PhoebeEditor.ViewModels;
using PhoebeEditor.Views.Pages;

namespace PhoebeEditor.Views;

public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    private readonly Dictionary<string, Page?> _pageCache = new()
    {
        ["menu"] = null,
        ["launcher"] = null,
        ["tweaks"] = null,
        ["presets"] = null
    };

    private string _currentPage = "menu";

    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
        DataContext = ViewModel;
        NavigateTo("menu");
    }

    private Page GetOrCreatePage(string page) => page switch
    {
        "menu" => _pageCache["menu"] ??= new PhoebeEditor.Views.Pages.Menu(ViewModel),
        "launcher" => _pageCache["launcher"] ??= new LauncherPage(ViewModel),
        "tweaks" => _pageCache["tweaks"] ??= new TweaksPage(ViewModel.Tweaks),
        "presets" => _pageCache["presets"] ??= new PresetsPage(ViewModel),
        _ => throw new ArgumentException($"Unknown page: {page}")
    };

    private void NavigateTo(string page)
    {
        _currentPage = page;
        MainFrame.Navigate(GetOrCreatePage(page));

        foreach (var name in new[] { "BtnMenu", "BtnLauncher", "BtnTweaks", "BtnPresets" })
        {
            if (FindName(name) is not Border border) continue;
            border.Background = Brushes.Transparent;
            foreach (var tb in GetTextBlocks(border))
                tb.Foreground = (SolidColorBrush)FindResource("WuWaTextMutedBrush");
        }

        var activeName = page switch
        {
            "menu" => "BtnMenu",
            "launcher" => "BtnLauncher",
            "tweaks" => "BtnTweaks",
            "presets" => "BtnPresets",
            _ => null
        };

        if (activeName != null && FindName(activeName) is Border active)
        {
            active.Background = (SolidColorBrush)FindResource("WuWaSurface2Brush");
            foreach (var tb in GetTextBlocks(active))
                tb.Foreground = (SolidColorBrush)FindResource("WuWaGoldBrush");
        }
    }

    private static IEnumerable<TextBlock> GetTextBlocks(Border border)
    {
        if (border.Child is StackPanel sp)
            return sp.Children.OfType<TextBlock>();
        return [];
    }

    private void NavItem_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is string tag)
            NavigateTo(tag);
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;
}