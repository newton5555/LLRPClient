using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using ControlzEx.Theming;

namespace LLRPReaderUI_WPF.Services;

public enum AppTheme
{
    Light,
    Dark
}

public class ThemeService
{
    private static readonly string ConfigFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LLRPReaderUI_WPF",
        "theme_config.json"
    );

    private AppTheme _currentTheme;

    public ThemeService()
    {
        _currentTheme = LoadThemeFromConfig();
        Initialize();
    }

    public AppTheme CurrentTheme
    {
        get => _currentTheme;
        private set
        {
            if (_currentTheme != value)
            {
                _currentTheme = value;
                SaveThemeToConfig(value);
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
        if (CurrentTheme != AppTheme.Light)
        {
            ApplyTheme(CurrentTheme);
        }
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

        // Step 1: Remove existing custom theme resource first
        for (int i = dictionaries.Count - 1; i >= 0; i--)
        {
            var dict = dictionaries[i];
            if (dict.Source != null && dict.Source.ToString().Contains("/Themes/"))
            {
                dictionaries.RemoveAt(i);
            }
        }

        // Step 2: Use ControlzEx ThemeManager to switch the MahApps base theme
        var themeName = theme == AppTheme.Light ? "Light.Blue" : "Dark.Blue";
        ThemeManager.Current.ChangeTheme(app, themeName);

        // Step 3: Add new custom theme resource (after MahApps theme is set)
        var themeDict = new ResourceDictionary { Source = ThemeUris[theme] };
        dictionaries.Add(themeDict);
    }

    private static AppTheme LoadThemeFromConfig()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<ThemeConfig>(json);
                if (config != null && Enum.IsDefined(typeof(AppTheme), config.Theme))
                {
                    return config.Theme;
                }
            }
        }
        catch
        {
            // 如果读取失败，使用默认主题
        }

        return AppTheme.Light;
    }

    private static void SaveThemeToConfig(AppTheme theme)
    {
        try
        {
            var directory = Path.GetDirectoryName(ConfigFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var config = new ThemeConfig { Theme = theme };
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
        }
        catch
        {
            // 保存失败时忽略
        }
    }

    private class ThemeConfig
    {
        public AppTheme Theme { get; set; }
    }
}
