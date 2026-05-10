using System.Windows;

namespace PhoebeEditor;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var splash = new Views.SplashScreen();
        splash.ShowWithFade();

        var updateService = new Services.UpdateService();
        splash.SetStatus("Checking for updates...");

        var updateResult = await updateService.CheckForUpdatesAsync();
        splash.SetStatus(updateResult.HasUpdate ? $"Update available: {updateResult.LatestVersion}" : "Ready!");

        var mainWindow = new Views.MainWindow();

        if (updateResult.HasUpdate)
        {
            mainWindow.UpdateAvailable = true;
            mainWindow.UpdateUrl = updateResult.DownloadUrl;
            mainWindow.UpdateVersion = updateResult.LatestVersion;
        }

        await System.Threading.Tasks.Task.Delay(800);

        splash.CloseWithFade();
        mainWindow.Show();
    }
}
