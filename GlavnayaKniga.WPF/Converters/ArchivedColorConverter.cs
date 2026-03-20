using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GlavnayaKniga.WPF.Converters
{
    public class ArchivedColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isArchived && isArchived)
            {
                return new SolidColorBrush(Colors.Gray);
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}