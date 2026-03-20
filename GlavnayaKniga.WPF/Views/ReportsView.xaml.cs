using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GlavnayaKniga.WPF.Views
{
    public partial class ReportsView : UserControl
    {
        public ReportsView()
        {
            InitializeComponent();
        }
    }

    // Конвертер для отступа в зависимости от уровня иерархии
    public class LevelToIndentConverter : IValueConverter
    {
        public static readonly LevelToIndentConverter Instance = new LevelToIndentConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int level)
            {
                return new Thickness(level * 20, 0, 0, 0);
            }
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Конвертер для форматирования периода
    public class DateRangeFormatter : IValueConverter
    {
        public static readonly DateRangeFormatter Instance = new DateRangeFormatter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime startDate && parameter is string format)
            {
                return startDate.ToString("dd.MM.yyyy");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}