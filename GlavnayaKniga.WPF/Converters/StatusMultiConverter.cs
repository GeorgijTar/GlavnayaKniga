using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GlavnayaKniga.WPF.Converters
{
    public class StatusMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0 || values[0] == null)
                return "Неизвестно";

            string status = values[0].ToString() ?? string.Empty;

            // Если запросили цвет фона
            if (parameter?.ToString() == "Background")
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

            // Если запросили цвет текста
            if (parameter?.ToString() == "Foreground")
            {
                return status switch
                {
                    "New" => new SolidColorBrush(Colors.Black),
                    "PartiallyProcessed" => new SolidColorBrush(Colors.Black),
                    "Processed" => new SolidColorBrush(Colors.White),
                    "Error" => new SolidColorBrush(Colors.White),
                    "Duplicate" => new SolidColorBrush(Colors.White),
                    _ => new SolidColorBrush(Colors.White)
                };
            }

            // По умолчанию возвращаем текст на русском
            return status switch
            {
                "New" => "Новая",
                "PartiallyProcessed" => "Частично обработана",
                "Processed" => "Обработана",
                "Error" => "Ошибка",
                "Duplicate" => "Дубликат",
                _ => status
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}