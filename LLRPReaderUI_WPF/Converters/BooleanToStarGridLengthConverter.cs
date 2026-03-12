using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LLRPReaderUI_WPF.Converters;

public sealed class BooleanToStarGridLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool visible && visible)
        {
            return new GridLength(1, GridUnitType.Star);
        }

        return new GridLength(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is GridLength gridLength)
        {
            return gridLength.Value > 0;
        }

        return false;
    }
}
