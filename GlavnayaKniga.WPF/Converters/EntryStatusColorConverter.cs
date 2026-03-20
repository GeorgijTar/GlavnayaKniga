using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GlavnayaKniga.WPF.Converters
{
    public class EntryStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Если значение null, возвращаем серый
            if (value == null)
            {
                return new SolidColorBrush(Colors.Gray);
            }

            // Если значение - int (не null)
            if (value is int intValue)
            {
                // Если ID больше 0, считаем что проводка создана - зеленый
                if (intValue > 0)
                {
                    return new SolidColorBrush(Colors.Green);
                }
            }

            // Во всех остальных случаях - серый
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}