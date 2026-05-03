using System.Windows;

namespace PhoebeEditor;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var mainWindow = new Views.MainWindow();
        mainWindow.Show();
    }
}
