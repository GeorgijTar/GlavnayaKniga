using System;
using System.Globalization;
using System.Windows.Data;
using GlavnayaKniga.Domain.Entities;

namespace GlavnayaKniga.WPF.Converters
{
    public class AccountTypeDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";

            // Пробуем преобразовать из int
            if (value is int intValue)
            {
                return intValue switch
                {
                    1 => "(дебетовое сальдо)",
                    2 => "(кредитовое сальдо)",
                    3 => "(дебетовое или кредитовое сальдо)",
                    _ => ""
                };
            }

            // Пробуем преобразовать из enum
            if (value is AccountType type)
            {
                return type switch
                {
                    AccountType.Active => "(дебетовое сальдо)",
                    AccountType.Passive => "(кредитовое сальдо)",
                    AccountType.ActivePassive => "(дебетовое или кредитовое сальдо)",
                    _ => ""
                };
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}