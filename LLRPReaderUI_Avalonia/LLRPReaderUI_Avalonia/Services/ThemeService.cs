using System.Text.Json;
using Avalonia;
using Avalonia.Styling;

namespace LLRPReaderUI_Avalonia.Services;

public enum AppTheme
{
    Light,
    Dark
}

public class ThemeService
{
    private static readonly string ConfigFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LLRPReaderUI_Avalonia",
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

        app.RequestedThemeVariant = theme == AppTheme.Light
            ? ThemeVariant.Light
            : ThemeVariant.Dark;
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
        }
    }

    private class ThemeConfig
    {
        public AppTheme Theme { get; set; }
    }
}
