using System;
using System.Globalization;
using System.Windows.Data;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.WPF.Converters
{
    public class AccountTypeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "Не выбран";

            // Пробуем преобразовать из int (если приходит число)
            if (value is int intValue)
            {
                return intValue switch
                {
                    1 => "Активный",
                    2 => "Пассивный",
                    3 => "Активно-пассивный",
                    _ => $"Неизвестный ({intValue})"
                };
            }

            // Пробуем преобразовать из строки
            if (value is string stringValue)
            {
                return stringValue switch
                {
                    "Active" => "Активный",
                    "Passive" => "Пассивный",
                    "ActivePassive" => "Активно-пассивный",
                    _ => stringValue
                };
            }

            // Пробуем преобразовать из enum
            if (value is AccountType type)
            {
                return type switch
                {
                    AccountType.Active => "Активный",
                    AccountType.Passive => "Пассивный",
                    AccountType.ActivePassive => "Активно-пассивный",
                    _ => "Неизвестный"
                };
            }

            return $"Неизвестный ({value?.GetType().Name})";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue switch
                {
                    "Активный" => AccountType.Active,
                    "Пассивный" => AccountType.Passive,
                    "Активно-пассивный" => AccountType.ActivePassive,
                    _ => AccountType.ActivePassive
                };
            }
            return AccountType.ActivePassive;
        }
    }
}