using System;
using System.Globalization;
using System.Windows.Data;

namespace GlavnayaKniga.WPF.Converters
{
    public class EntryStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Если значение null, возвращаем "Не создана"
            if (value == null)
            {
                return "○ Не создана";
            }

            // Если значение - int (не null)
            if (value is int intValue)
            {
                // Если ID больше 0, считаем что проводка создана
                if (intValue > 0)
                {
                    return "✓ Создана";
                }
            }

            // Во всех остальных случаях
            return "○ Не создана";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}