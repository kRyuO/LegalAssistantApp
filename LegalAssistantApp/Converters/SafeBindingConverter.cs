using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia;

namespace LegalAssistantApp.Converters
{
    /// <summary>
    /// Конвертер для безопасной привязки к свойствам объектов, которые могут быть null
    /// </summary>
    public class SafeBindingConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                // Возвращаем значение по умолчанию в зависимости от типа
                if (targetType == typeof(string))
                    return string.Empty;
                if (targetType == typeof(bool) || targetType == typeof(bool?))
                    return false;
                if (targetType == typeof(int) || targetType == typeof(int?))
                    return 0;
                if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                    return 0m;
                if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                    return null;
                return AvaloniaProperty.UnsetValue;
            }

            // Если параметр указан как путь к свойству (например, "Title")
            if (parameter is string propertyPath && value != null)
            {
                try
                {
                    var prop = value.GetType().GetProperty(propertyPath);
                    if (prop != null)
                    {
                        var propValue = prop.GetValue(value);
                        return propValue ?? GetDefaultValue(targetType);
                    }
                }
                catch
                {
                    return GetDefaultValue(targetType);
                }
            }

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }

        private object? GetDefaultValue(Type type)
        {
            if (type == typeof(string))
                return string.Empty;
            if (type == typeof(bool) || type == typeof(bool?))
                return false;
            if (type == typeof(int) || type == typeof(int?))
                return 0;
            if (type == typeof(decimal) || type == typeof(decimal?))
                return 0m;
            return null;
        }
    }
}

