using Avalonia;
using Avalonia.Styling;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LegalAssistantApp.Themes;

public class ThemeManager
{
    private static ThemeManager? _instance;
    private Application? _application;
    private Dictionary<string, ThemeColors> _themes;

    public static ThemeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ThemeManager();
            }
            return _instance;
        }
    }

    private ThemeManager()
    {
        _themes = new Dictionary<string, ThemeColors>();
        InitializeThemes();
    }

    public void Initialize(Application application)
    {
        _application = application;
    }

    private void InitializeThemes()
    {
        _themes["Light"] = new ThemeColors
        {
            WindowBackground = "#f8faf8",
            SidebarBackground = "#2d3e36",
            CardBackground = "#ffffff",
            PrimaryColor = "#5a8a7a",
            SecondaryColor = "#e8f0ed",
            TextPrimary = "#2d3e36",
            TextSecondary = "#5a6a62",
            TextMuted = "#8a9a92",
            BorderColor = "#d9e2e0",
            HoverBackground = "#f1f8f4",
            SelectedBackground = "#e8f5e9"
        };

        _themes["Dark"] = new ThemeColors
        {
            WindowBackground = "#1a1a1a",
            SidebarBackground = "#0d1411",
            CardBackground = "#2d2d2d",
            PrimaryColor = "#4a7a6a",
            SecondaryColor = "#3a3a3a",
            TextPrimary = "#e0e0e0",
            TextSecondary = "#b0b0b0",
            TextMuted = "#808080",
            BorderColor = "#404040",
            HoverBackground = "#353535",
            SelectedBackground = "#2a4a3a"
        };
    }

    public void ApplyTheme(string themeName)
    {
        if (!_themes.TryGetValue(themeName, out var colors))
        {
            colors = _themes["Light"];
        }

        if (_application != null)
        {
            var resources = _application.Resources;
            if (resources == null)
            {
                resources = new Avalonia.Controls.ResourceDictionary();
                _application.Resources = resources;
            }

            // В ресурсах храним именно кисти, иначе Avalonia не сможет применить значения к Background/Foreground/BorderBrush
            resources["ThemeWindowBackground"] = Brush.Parse(colors.WindowBackground);
            resources["ThemeSidebarBackground"] = Brush.Parse(colors.SidebarBackground);
            resources["ThemeCardBackground"] = Brush.Parse(colors.CardBackground);
            resources["ThemePrimaryColor"] = Brush.Parse(colors.PrimaryColor);
            resources["ThemeSecondaryColor"] = Brush.Parse(colors.SecondaryColor);
            resources["ThemeTextPrimary"] = Brush.Parse(colors.TextPrimary);
            resources["ThemeTextSecondary"] = Brush.Parse(colors.TextSecondary);
            resources["ThemeTextMuted"] = Brush.Parse(colors.TextMuted);
            resources["ThemeBorderColor"] = Brush.Parse(colors.BorderColor);
            resources["ThemeHoverBackground"] = Brush.Parse(colors.HoverBackground);
            resources["ThemeSelectedBackground"] = Brush.Parse(colors.SelectedBackground);

             if (themeName == "Dark")
            {
                _application.RequestedThemeVariant = ThemeVariant.Dark;
            }
            else
            {
                _application.RequestedThemeVariant = ThemeVariant.Light;
            }
        }
    }

    public ThemeColors GetCurrentTheme()
    {
        return _themes.ContainsKey("Light") ? _themes["Light"] : _themes.Values.First();
    }
}

public class ThemeColors
{
    public string WindowBackground { get; set; } = "#f8faf8";
    public string SidebarBackground { get; set; } = "#2d3e36";
    public string CardBackground { get; set; } = "#ffffff";
    public string PrimaryColor { get; set; } = "#5a8a7a";
    public string SecondaryColor { get; set; } = "#e8f0ed";
    public string TextPrimary { get; set; } = "#2d3e36";
    public string TextSecondary { get; set; } = "#5a6a62";
    public string TextMuted { get; set; } = "#8a9a92";
    public string BorderColor { get; set; } = "#d9e2e0";
    public string HoverBackground { get; set; } = "#f1f8f4";
    public string SelectedBackground { get; set; } = "#e8f5e9";
}

