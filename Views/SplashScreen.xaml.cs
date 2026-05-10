using System.Reflection;
using System.Windows;
using System.Windows.Media.Animation;

namespace PhoebeEditor.Views;

public partial class SplashScreen : Window
{
    public SplashScreen()
    {
        InitializeComponent();

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = $"v{version?.ToString(3) ?? "0.0.0"}";
    }

    public void ShowWithFade()
    {
        Show();
        var fadeIn = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.6),
            EasingFunction = new QuadraticEase()
        };
        BeginAnimation(OpacityProperty, fadeIn);
    }

    public void CloseWithFade()
    {
        var fadeOut = new DoubleAnimation
        {
            From = Opacity,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.4),
            EasingFunction = new QuadraticEase()
        };
        fadeOut.Completed += (_, _) => Close();
        BeginAnimation(OpacityProperty, fadeOut);
    }

    public void SetStatus(string status) => Dispatcher.Invoke(() => StatusText.Text = status);
}
