using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LegalAssistantApp.Converters
{
    public class DecimalConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return "0";

            // Проверяем decimal (включая nullable)
            try
            {
                var decValue = System.Convert.ToDecimal(value);
                return decValue.ToString("F2", culture);
            }
            catch
            {
                // Если не удалось преобразовать, возвращаем строковое представление
                return value.ToString() ?? "0";
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return null;

            if (decimal.TryParse(value.ToString(), NumberStyles.Any, culture, out var result))
            {
                return result;
            }

            return null;
        }
    }
}

