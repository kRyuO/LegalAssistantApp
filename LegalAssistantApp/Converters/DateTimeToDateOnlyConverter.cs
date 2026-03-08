using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LegalAssistantApp.Converters
{
    public class DateTimeToDateOnlyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value is DateTime dateTime)
            {
                // Конвертируем DateTime в DateOnly (только дата без времени)
                return DateOnly.FromDateTime(dateTime);
            }

            if (value is DateTimeOffset dateTimeOffset)
            {
                return DateOnly.FromDateTime(dateTimeOffset.DateTime);
            }

            // Если это строка, пытаемся распарсить
            if (value is string str && DateTime.TryParse(str, out var parsed))
            {
                return DateOnly.FromDateTime(parsed);
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value is DateOnly dateOnly)
            {
                return dateOnly.ToDateTime(TimeOnly.MinValue);
            }

            if (value is DateTime dateTime)
            {
                // Убираем время, оставляем только дату
                return dateTime.Date;
            }

            if (value is DateTimeOffset dateTimeOffset)
            {
                return dateTimeOffset.DateTime.Date;
            }

            return null;
        }
    }
}

