using System;
using System.Globalization;
using System.Windows.Data;

namespace GlavnayaKniga.WPF.Converters
{
    public class BooleanToTagConverter : IValueConverter
    {
        public static readonly BooleanToTagConverter Instance = new BooleanToTagConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Valid" : "Invalid";
            }
            return "Invalid";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}