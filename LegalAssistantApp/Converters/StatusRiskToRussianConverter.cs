using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LegalAssistantApp.Converters
{
    /// <summary>
    /// Отображает статус и уровень риска контрагента по-русски (Active/Low/Medium/High и т.д.).
    /// </summary>
    public class StatusRiskToRussianConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string s || string.IsNullOrEmpty(s))
                return value;

            // Статус
            if (s.Equals("Active", StringComparison.OrdinalIgnoreCase)) return "Активен";
            if (s.Equals("Inactive", StringComparison.OrdinalIgnoreCase)) return "Неактивен";
            if (s.Equals("Blocked", StringComparison.OrdinalIgnoreCase)) return "Заблокирован";

            // Уровень риска
            if (s.Equals("Low", StringComparison.OrdinalIgnoreCase)) return "Низкий";
            if (s.Equals("Medium", StringComparison.OrdinalIgnoreCase)) return "Средний";
            if (s.Equals("High", StringComparison.OrdinalIgnoreCase)) return "Высокий";
            if (s.Equals("Unknown", StringComparison.OrdinalIgnoreCase)) return "Неизвестен";

            return s;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
