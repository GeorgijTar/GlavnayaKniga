using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GlavnayaKniga.WPF.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "New" => new SolidColorBrush(Colors.Orange),
                    "PartiallyProcessed" => new SolidColorBrush(Colors.Gold),
                    "Processed" => new SolidColorBrush(Colors.Green),
                    "Error" => new SolidColorBrush(Colors.Red),
                    "Duplicate" => new SolidColorBrush(Colors.Gray),
                    _ => new SolidColorBrush(Colors.Blue)
                };
            }

            // Поддержка enum
            if (value is Enum enumValue)
            {
                return enumValue.ToString() switch
                {
                    "New" => new SolidColorBrush(Colors.Orange),
                    "PartiallyProcessed" => new SolidColorBrush(Colors.Gold),
                    "Processed" => new SolidColorBrush(Colors.Green),
                    "Error" => new SolidColorBrush(Colors.Red),
                    "Duplicate" => new SolidColorBrush(Colors.Gray),
                    _ => new SolidColorBrush(Colors.Blue)
                };
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}