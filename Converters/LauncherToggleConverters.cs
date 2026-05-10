using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PhoebeEditor.Models;

namespace PhoebeEditor.Converters;

public class LauncherToSteamBgConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (LauncherType)value == LauncherType.Steam
            ? (SolidColorBrush)System.Windows.Application.Current.FindResource("WuWaSurface2Brush")
            : System.Windows.Media.Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public class LauncherToSteamBorderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (LauncherType)value == LauncherType.Steam
            ? (SolidColorBrush)System.Windows.Application.Current.FindResource("WuWaGoldBrush")
            : (SolidColorBrush)System.Windows.Application.Current.FindResource("WuWaBorderBrush");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public class LauncherToSteamTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (LauncherType)value == LauncherType.Steam
            ? (SolidColorBrush)System.Windows.Application.Current.FindResource("WuWaGoldBrush")
            : (SolidColorBrush)System.Windows.Application.Current.FindResource("WuWaTextMutedBrush");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public class LauncherToKuroBgConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (LauncherType)value == LauncherType.Kuro
            ? (SolidColorBrush)System.Windows.Application.Current.FindResource("WuWaSurface2Brush")
            : System.Windows.Media.Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public class LauncherToKuroBorderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (LauncherType)value == LauncherType.Kuro
            ? (SolidColorBrush)System.Windows.Application.Current.FindResource("WuWaGoldBrush")
            : (SolidColorBrush)System.Windows.Application.Current.FindResource("WuWaBorderBrush");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public class LauncherToKuroTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (LauncherType)value == LauncherType.Kuro
            ? (SolidColorBrush)System.Windows.Application.Current.FindResource("WuWaGoldBrush")
            : (SolidColorBrush)System.Windows.Application.Current.FindResource("WuWaTextMutedBrush");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
