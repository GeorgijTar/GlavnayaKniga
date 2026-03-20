using System;
using System.Globalization;
using System.Windows.Data;

namespace GlavnayaKniga.WPF.Converters
{
    public class StatusToRussianConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "Неизвестно";

            // Если пришла строка
            if (value is string statusString)
            {
                return statusString switch
                {
                    "New" => "Новая",
                    "PartiallyProcessed" => "Частично обработана",
                    "Processed" => "Обработана",
                    "Error" => "Ошибка",
                    "Duplicate" => "Дубликат",
                    _ => statusString
                };
            }

            // Если пришел enum
            if (value is Enum enumValue)
            {
                return enumValue.ToString() switch
                {
                    "New" => "Новая",
                    "PartiallyProcessed" => "Частично обработана",
                    "Processed" => "Обработана",
                    "Error" => "Ошибка",
                    "Duplicate" => "Дубликат",
                    _ => enumValue.ToString()
                };
            }

            return value.ToString() ?? "Неизвестно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}