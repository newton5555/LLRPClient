using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace LLRPReaderUI_WPF.Converters
{
    public class GpioStateColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return DependencyProperty.UnsetValue;
            
            string state = value.ToString();
            
            if (state.Contains("High") || state.Contains("高"))
            {
                return Application.Current.TryFindResource("IconSuccessBrush") as Brush ?? Brushes.Green;
            }
            if (state.Contains("Low") || state.Contains("低"))
            {
                return Application.Current.TryFindResource("TextMutedBrush") as Brush ?? Brushes.Gray;
            }
            
            return Application.Current.TryFindResource("TextPrimaryBrush") as Brush ?? Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
