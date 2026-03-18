using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LLRPReaderUI_WPF.Converters
{
    public class DirectionToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string direction)
            {
                return direction switch
                {
                    "SENT" => Brushes.Green,
                    "RECEIVED" => Brushes.Blue,
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
