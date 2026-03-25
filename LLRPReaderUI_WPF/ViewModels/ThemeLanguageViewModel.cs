using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FontAwesome.Sharp;
using LLRPReaderUI_WPF.Services;

namespace LLRPReaderUI_WPF.ViewModels;

public partial class ThemeLanguageViewModel : ObservableObject
{
    private readonly ThemeService _themeService;
    private readonly LanguageService _languageService;

    public ThemeLanguageViewModel(ThemeService themeService, LanguageService languageService)
    {
        _themeService = themeService;
        _languageService = languageService;

        // 订阅主题变化事件
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

    /// <summary>
    /// 当前主题对应的图标：亮色主题显示太阳，暗色主题显示月亮
    /// </summary>
    public IconChar ThemeIcon => _themeService.CurrentTheme == AppTheme.Light
        ? IconChar.Sun
        : IconChar.Moon;

    /// <summary>
    /// 当前语言对应的图标：中文用亚洲地球，英文用美洲地球
    /// </summary>
    public IconChar LanguageIcon => _languageService.CurrentLanguage == AppLanguage.ZhCN
        ? IconChar.EarthAsia
        : IconChar.EarthAmericas;

    /// <summary>
    /// 当前语言的文字符号
    /// </summary>
    public string LanguageSymbol => _languageService.CurrentLanguage == AppLanguage.ZhCN
        ? "中"
        : "EN";

    /// <summary>
    /// 主题切换按钮的工具提示
    /// </summary>
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
