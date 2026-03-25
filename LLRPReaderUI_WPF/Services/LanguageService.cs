using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace LLRPReaderUI_WPF.Services;

public enum AppLanguage
{
    EnUS,
    ZhCN
}

public class LanguageService
{
    private static readonly string ConfigFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LLRPReaderUI_WPF",
        "language_config.json"
    );

    private AppLanguage _currentLanguage;

    public LanguageService()
    {
        _currentLanguage = LoadLanguageFromConfig();
        Initialize();
    }

    public AppLanguage CurrentLanguage
    {
        get => _currentLanguage;
        private set
        {
            if (_currentLanguage != value)
            {
                _currentLanguage = value;
                SaveLanguageToConfig(value);
                OnLanguageChanged?.Invoke(value);
            }
        }
    }

    public event Action<AppLanguage>? OnLanguageChanged;

    private static readonly Dictionary<AppLanguage, Uri> LanguageUris = new()
    {
        [AppLanguage.EnUS] = new Uri("pack://application:,,,/Resources/Localization/Strings.en-US.xaml", UriKind.Absolute),
        [AppLanguage.ZhCN] = new Uri("pack://application:,,,/Resources/Localization/Strings.zh-CN.xaml", UriKind.Absolute)
    };

    public void Initialize()
    {
        ApplyLanguage(CurrentLanguage);
    }

    public void SetLanguage(AppLanguage language)
    {
        if (CurrentLanguage == language) return;
        CurrentLanguage = language;
        ApplyLanguage(language);
    }

    private void ApplyLanguage(AppLanguage language)
    {
        var app = Application.Current;
        if (app is null) return;

        var dictionaries = app.Resources.MergedDictionaries;

        // Remove existing language resource
        for (int i = dictionaries.Count - 1; i >= 0; i--)
        {
            var dict = dictionaries[i];
            if (dict.Source != null && dict.Source.ToString().Contains("/Localization/"))
            {
                dictionaries.RemoveAt(i);
                break;
            }
        }

        // Add new language resource
        var langDict = new ResourceDictionary { Source = LanguageUris[language] };
        dictionaries.Add(langDict);
    }

    public string GetLocalizedString(string key)
    {
        var app = Application.Current;
        if (app is null) return key;

        if (app.Resources.Contains(key) && app.Resources[key] is string value)
        {
            return value;
        }

        return key;
    }

    private static AppLanguage LoadLanguageFromConfig()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<LanguageConfig>(json);
                if (config != null && Enum.IsDefined(typeof(AppLanguage), config.Language))
                {
                    return config.Language;
                }
            }
        }
        catch
        {
            // If loading fails, use default language
        }

        // Default to system language or Chinese
        var systemLang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return systemLang == "zh" ? AppLanguage.ZhCN : AppLanguage.EnUS;
    }

    private static void SaveLanguageToConfig(AppLanguage language)
    {
        try
        {
            var directory = Path.GetDirectoryName(ConfigFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var config = new LanguageConfig { Language = language };
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }

    private class LanguageConfig
    {
        public AppLanguage Language { get; set; }
    }
}
