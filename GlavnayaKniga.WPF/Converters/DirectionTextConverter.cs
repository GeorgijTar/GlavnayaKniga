using System;
using System.Globalization;
using System.Windows.Data;

namespace GlavnayaKniga.WPF.Converters
{
    public class DirectionTextConverter : IValueConverter
    {
        public static readonly DirectionTextConverter Instance = new DirectionTextConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isIncoming)
            {
                return isIncoming ? "Входящий платеж" : "Исходящий платеж";
            }
            return "Неизвестно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}