using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LegalAssistantApp.Converters
{
    public class NotNullToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var result = value != null;
            // Если параметр "Invert", инвертируем результат
            if (parameter?.ToString() == "Invert")
            {
                result = !result;
            }
            return result;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}