using Avalonia.Markup.Xaml;
using System;

namespace LegalAssistantApp.Localization;

public class LocalizationExtension : MarkupExtension
{
    public string Key { get; set; } = string.Empty;
    public object[]? Args { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key))
            return string.Empty;

        var loc = LocalizationService.Instance;
        
        if (Args != null && Args.Length > 0)
        {
            return loc.GetString(Key, Args);
        }
        
        return loc[Key];
    }
}

