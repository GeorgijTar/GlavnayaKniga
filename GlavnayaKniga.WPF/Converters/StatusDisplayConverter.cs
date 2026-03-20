using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GlavnayaKniga.WPF.Converters
{
    public class StatusDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new StatusDisplayInfo();

            string status = value.ToString() ?? string.Empty;

            return status switch
            {
                "New" => new StatusDisplayInfo
                {
                    Text = "Новая",
                    Color = Colors.Orange,
                    BackgroundColor = Colors.Orange,
                    ForegroundColor = Colors.Black
                },
                "PartiallyProcessed" => new StatusDisplayInfo
                {
                    Text = "Частично обработана",
                    Color = Colors.Gold,
                    BackgroundColor = Colors.Gold,
                    ForegroundColor = Colors.Black
                },
                "Processed" => new StatusDisplayInfo
                {
                    Text = "Обработана",
                    Color = Colors.Green,
                    BackgroundColor = Colors.Green,
                    ForegroundColor = Colors.White
                },
                "Error" => new StatusDisplayInfo
                {
                    Text = "Ошибка",
                    Color = Colors.Red,
                    BackgroundColor = Colors.Red,
                    ForegroundColor = Colors.White
                },
                "Duplicate" => new StatusDisplayInfo
                {
                    Text = "Дубликат",
                    Color = Colors.Gray,
                    BackgroundColor = Colors.Gray,
                    ForegroundColor = Colors.White
                },
                _ => new StatusDisplayInfo
                {
                    Text = status,
                    Color = Colors.Blue,
                    BackgroundColor = Colors.Blue,
                    ForegroundColor = Colors.White
                }
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusDisplayInfo
    {
        public string Text { get; set; } = string.Empty;
        public Color Color { get; set; }
        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }
    }
}