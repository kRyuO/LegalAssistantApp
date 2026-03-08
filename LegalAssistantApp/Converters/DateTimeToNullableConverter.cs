using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LegalAssistantApp.Converters
{
    /// <summary>
    /// Конвертер для преобразования DateTime в DateTimeOffset? для DatePicker в Avalonia
    /// </summary>
    public class DateTimeToNullableConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            try
            {
                DateTime dateTime;
                
                if (value is DateTime dt)
                {
                    dateTime = dt;
                }
                else if (value is DateTimeOffset dto)
                {
                    dateTime = dto.DateTime;
                }
                else if (value is string str && DateTime.TryParse(str, out var parsed))
                {
                    dateTime = parsed;
                }
                else
                {
                    return null;
                }

                // Возвращаем только дату без времени как DateTimeOffset
                var dateOnly = dateTime.Date;
                return new DateTimeOffset(dateOnly.Year, dateOnly.Month, dateOnly.Day, 0, 0, 0, TimeSpan.Zero);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в DateTimeToNullableConverter.Convert: {ex}");
                return null;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            try
            {
                DateTime dateTime;
                
                if (value is DateTimeOffset dto)
                {
                    dateTime = dto.DateTime;
                }
                else if (value is DateTime dt)
                {
                    dateTime = dt;
                }
                else
                {
                    return null;
                }

                // Возвращаем только дату без времени
                var dateOnly = dateTime.Date;
                return new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day, 0, 0, 0, DateTimeKind.Unspecified);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка в DateTimeToNullableConverter.ConvertBack: {ex}");
                return null;
            }
        }
    }
}

