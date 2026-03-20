using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GlavnayaKniga.WPF.Converters
{
    /// <summary>
    /// Конвертер для видимости кнопки редактирования (недоступна для уволенных)
    /// </summary>
    public class CanEditVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                // Скрываем кнопку для уволенных сотрудников
                return status != "Dismissed" ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для видимости кнопки перевода (недоступна для уволенных)
    /// </summary>
    public class CanTransferVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                // Скрываем кнопку для уволенных сотрудников
                return status != "Dismissed" ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для видимости кнопки увольнения (недоступна для уже уволенных)
    /// </summary>
    public class CanDismissVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                // Скрываем кнопку для уже уволенных сотрудников
                return status != "Dismissed" ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер статуса сотрудника в цвет фона
    /// </summary>
    public class EmployeeStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Active" => "#4CAF50",
                    "Probation" => "#FF9800",
                    "OnLeave" => "#2196F3",
                    "Dismissed" => "#9E9E9E",
                    _ => "#607D8B"
                };
            }
            return "#607D8B";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер статуса сотрудника в текст на русском
    /// </summary>
    public class EmployeeStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Active" => "Работает",
                    "Probation" => "Исп. срок",
                    "OnLeave" => "В отпуске",
                    "Dismissed" => "Уволен",
                    _ => "Неизвестно"
                };
            }
            return "Неизвестно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}