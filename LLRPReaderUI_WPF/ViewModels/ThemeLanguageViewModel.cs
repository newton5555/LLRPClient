using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
}
