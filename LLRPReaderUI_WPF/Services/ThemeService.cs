using System;
using System.Collections.Generic;
using System.Windows;

namespace LLRPReaderUI_WPF.Services;

public enum AppTheme
{
    Light,
    Dark
}

public class ThemeService
{
    public ThemeService()
    {
        Initialize();  // 构造时自动调用
    }



    private AppTheme _currentTheme = AppTheme.Light;

    public AppTheme CurrentTheme
    {
        get => _currentTheme;
        private set
        {
            if (_currentTheme != value)
            {
                _currentTheme = value;
                OnThemeChanged?.Invoke(value);
            }
        }
    }

    public event Action<AppTheme>? OnThemeChanged;

    private static readonly Dictionary<AppTheme, Uri> ThemeUris = new()
    {
        [AppTheme.Light] = new Uri("pack://application:,,,/Themes/FlatTheme.xaml", UriKind.Absolute),
        [AppTheme.Dark] = new Uri("pack://application:,,,/Themes/DarkTheme.xaml", UriKind.Absolute)
    };

    public void Initialize()
    {
        ApplyTheme(CurrentTheme);
    }

    public void SetTheme(AppTheme theme)
    {
        if (CurrentTheme == theme) return;
        CurrentTheme = theme;
        ApplyTheme(theme);
    }

    private void ApplyTheme(AppTheme theme)
    {
        var app = Application.Current;
        if (app is null) return;

        var dictionaries = app.Resources.MergedDictionaries;

        // Remove existing theme resource
        for (int i = dictionaries.Count - 1; i >= 0; i--)
        {
            var dict = dictionaries[i];
            if (dict.Source != null && dict.Source.ToString().Contains("/Themes/"))
            {
                dictionaries.RemoveAt(i);
                break;
            }
        }

        // Add new theme resource
        var themeDict = new ResourceDictionary { Source = ThemeUris[theme] };
        dictionaries.Add(themeDict);
    }
}
