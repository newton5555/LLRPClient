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
    }

    private void OnThemeChanged(AppTheme theme)
    {
        OnPropertyChanged(nameof(CurrentTheme));
        OnPropertyChanged(nameof(ThemeIcon));
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
    /// 当前主题对应的图标：亮色主题显示灯泡，暗色主题显示月亮
    /// </summary>
    public IconChar ThemeIcon => _themeService.CurrentTheme == AppTheme.Light 
        ? IconChar.Sun 
        : IconChar.Moon;

    /// <summary>
    /// 主题切换按钮的工具提示
    /// </summary>
    public string ThemeToolTip => _themeService.CurrentTheme == AppTheme.Light 
        ? "切换到暗色主题" 
        : "切换到亮色主题";
}
