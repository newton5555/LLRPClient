using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;

namespace LLRPReaderUI_Avalonia.Services;

public enum AppLanguage
{
    EnUS,
    ZhCN
}

public class LanguageService : INotifyPropertyChanged
{
    private static readonly string ConfigFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LLRPReaderUI_Avalonia",
        "language_config.json"
    );

    private AppLanguage _currentLanguage;
    private int _languageDictIndex =0;

    private const string ZhCN_File = "avares://LLRPReaderUI_Avalonia/Resources/Localization/Strings.zh-CN.axaml";
    private const string EnUS_File = "avares://LLRPReaderUI_Avalonia/Resources/Localization/Strings.en-US.axaml";

    public AppLanguage CurrentLanguage => _currentLanguage;

    public event Action<AppLanguage>? OnLanguageChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public LanguageService()
    {
        _currentLanguage = LoadLanguageFromConfig();
    }

    public void Initialize()
    {
        FindLanguageDictionaryIndex();

        if (_currentLanguage != AppLanguage.ZhCN)
        {
            ApplyLanguage(_currentLanguage);
        }
    }

    private void FindLanguageDictionaryIndex()
    {
        var merged = Application.Current?.Resources?.MergedDictionaries;
        if (merged == null) return;

        for (int i = 0; i < merged.Count; i++)
        {
            if (merged[i] is ResourceDictionary dict && dict.ContainsKey("__LanguageResource__"))
            {
                _languageDictIndex = i;
                return;
            }
        }
    }

    public void SetLanguage(AppLanguage language)
    {
        if (_currentLanguage == language) return;

        _currentLanguage = language;
        SaveLanguageToConfig(language);
        ApplyLanguage(language);
        OnLanguageChanged?.Invoke(language);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
    }

    private void ApplyLanguage(AppLanguage language)
    {
        var file = language == AppLanguage.ZhCN ? ZhCN_File : EnUS_File;

        var newResource = new ResourceInclude(new Uri(file, UriKind.Absolute));
        newResource.Source = new Uri(file, UriKind.Absolute);

        Application.Current!.Resources.MergedDictionaries[_languageDictIndex] = newResource;
    }

    public string GetLocalizedString(string key)
    {
        try
        {
            Application.Current!.TryFindResource(key, out var value);
            return value as string ?? key;
        }
        catch
        {
            return key;
        }
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
        catch { }

        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "zh"
            ? AppLanguage.ZhCN
            : AppLanguage.EnUS;
    }

    private static void SaveLanguageToConfig(AppLanguage language)
    {
        try
        {
            var dir = Path.GetDirectoryName(ConfigFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(new LanguageConfig { Language = language },
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
        }
        catch { }
    }

    private class LanguageConfig
    {
        public AppLanguage Language { get; set; }
    }
}
