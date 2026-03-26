using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace LLRPReaderUI_Avalonia.Converters;

/// <summary>
/// Converts boolean status to appropriate background brush for status chips
/// </summary>
public class DeviceStatusConverter : IMultiValueConverter
{
    public static readonly DeviceStatusConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count > 0 && values[0] is bool isConnected)
        {
            return isConnected
                ? new SolidColorBrush(Color.Parse("#DCFCE7"))  // Success green background
                : new SolidColorBrush(Color.Parse("#FEE2E2")); // Error red background
        }
        return new SolidColorBrush(Color.Parse("#E2E8F0")); // Default gray
    }
}

/// <summary>
/// Converts boolean status to appropriate dot color for status indicators
/// </summary>
public class DeviceStatusDotConverter : IMultiValueConverter
{
    public static readonly DeviceStatusDotConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count > 0 && values[0] is bool isConnected)
        {
            return isConnected
                ? new SolidColorBrush(Color.Parse("#22C55E"))  // Success green dot
                : new SolidColorBrush(Color.Parse("#EF4444")); // Error red dot
        }
        return new SolidColorBrush(Color.Parse("#94A3B8")); // Default gray
    }
}

/// <summary>
/// Converts boolean status to appropriate text color
/// </summary>
public class DeviceStatusTextConverter : IMultiValueConverter
{
    public static readonly DeviceStatusTextConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count > 0 && values[0] is bool isConnected)
        {
            return isConnected
                ? new SolidColorBrush(Color.Parse("#166534"))  // Success green text
                : new SolidColorBrush(Color.Parse("#991B1B")); // Error red text
        }
        return new SolidColorBrush(Color.Parse("#475569")); // Default gray text
    }
}

/// <summary>
/// Converts inventory running status to appropriate background brush
/// </summary>
public class InventoryStatusConverter : IMultiValueConverter
{
    public static readonly InventoryStatusConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count > 0 && values[0] is bool isRunning)
        {
            return isRunning
                ? new SolidColorBrush(Color.Parse("#DBEAFE"))  // Active blue background
                : new SolidColorBrush(Color.Parse("#E2E8F0")); // Default gray background
        }
        return new SolidColorBrush(Color.Parse("#E2E8F0")); // Default gray
    }
}

/// <summary>
/// Converts inventory running status to appropriate dot color
/// </summary>
public class InventoryStatusDotConverter : IMultiValueConverter
{
    public static readonly InventoryStatusDotConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count > 0 && values[0] is bool isRunning)
        {
            return isRunning
                ? new SolidColorBrush(Color.Parse("#3B82F6"))  // Active blue dot
                : new SolidColorBrush(Color.Parse("#94A3B8")); // Default gray dot
        }
        return new SolidColorBrush(Color.Parse("#94A3B8")); // Default gray
    }
}

/// <summary>
/// Converts inventory running status to appropriate text color
/// </summary>
public class InventoryStatusTextConverter : IMultiValueConverter
{
    public static readonly InventoryStatusTextConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count > 0 && values[0] is bool isRunning)
        {
            return isRunning
                ? new SolidColorBrush(Color.Parse("#1D4ED8"))  // Active blue text
                : new SolidColorBrush(Color.Parse("#475569")); // Default gray text
        }
        return new SolidColorBrush(Color.Parse("#475569")); // Default gray text
    }
}
