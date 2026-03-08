using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LegalAssistantApp.Converters
{
    public class EventStatusToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Завершено" => "#4a8a6a",
                    "Активно" => "#5a8a7a",
                    "Отменено" => "#b35454",
                    "Перенесено" => "#b89a5a",
                    _ => "#8a9a92"
                };
            }
            return "#8a9a92";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}