using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LLRPReaderUI_Avalonia.Services;

namespace LLRPReaderUI_Avalonia.ViewModels;

public partial class ThemeLanguageViewModel : ViewModelBase
{
    private readonly ThemeService _themeService;
    private readonly LanguageService _languageService;

    public ThemeLanguageViewModel(ThemeService themeService, LanguageService languageService)
    {
        _themeService = themeService;
        _languageService = languageService;

        _themeService.OnThemeChanged += OnThemeChanged;
        _languageService.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnThemeChanged(AppTheme theme)
    {
        OnPropertyChanged(nameof(CurrentTheme));
        OnPropertyChanged(nameof(ThemeIcon));
        OnPropertyChanged(nameof(ThemeToolTip));
    }

    private void OnLanguageChanged(AppLanguage language)
    {
        OnPropertyChanged(nameof(CurrentLanguage));
        OnPropertyChanged(nameof(LanguageIcon));
        OnPropertyChanged(nameof(LanguageSymbol));
        OnPropertyChanged(nameof(ThemeToolTip));
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        var newTheme = _themeService.CurrentTheme == AppTheme.Light
            ? AppTheme.Dark
            : AppTheme.Light;
        _themeService.SetTheme(newTheme);
    }

    [RelayCommand]
    private void ToggleLanguage()
    {
        var newLanguage = _languageService.CurrentLanguage == AppLanguage.ZhCN
            ? AppLanguage.EnUS
            : AppLanguage.ZhCN;
        _languageService.SetLanguage(newLanguage);
    }

    public AppTheme CurrentTheme => _themeService.CurrentTheme;
    public AppLanguage CurrentLanguage => _languageService.CurrentLanguage;

    public MaterialIconKind ThemeIcon => _themeService.CurrentTheme == AppTheme.Light
        ? MaterialIconKind.WeatherSunny
        : MaterialIconKind.WeatherNight;

    public MaterialIconKind LanguageIcon => _languageService.CurrentLanguage == AppLanguage.ZhCN
        ? MaterialIconKind.Earth
        : MaterialIconKind.Earth;

    public string LanguageSymbol => _languageService.CurrentLanguage == AppLanguage.ZhCN
        ? "中"
        : "EN";

    public string ThemeToolTip
    {
        get
        {
            var key = _themeService.CurrentTheme == AppTheme.Light
                ? "Theme.ToggleDark"
                : "Theme.ToggleLight";
            return _languageService.GetLocalizedString(key);
        }
    }
}
