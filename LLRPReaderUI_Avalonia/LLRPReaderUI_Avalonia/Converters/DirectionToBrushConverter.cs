using System.Globalization;

namespace LLRPReaderUI_Avalonia.Converters
{
    public class DirectionToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string direction)
            {
                return direction switch
                {
                    "SENT" or "TX" => Brushes.Green,
                    "RECEIVED" or "RX" => Brushes.Blue,
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return BindingOperations.DoNothing;
        }
    }
}
