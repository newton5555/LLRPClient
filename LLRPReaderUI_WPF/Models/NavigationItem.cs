namespace LLRPReaderUI_WPF.Models;

using FontAwesome.Sharp;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

public sealed class NavigationItem : INotifyPropertyChanged
{
    private string _title = string.Empty;

    public required string Title
    {
        get => _title;
        init => SetProperty(ref _title, value);
    }

    // Resource key for localization (e.g., "Menu.DeviceConnection")
    public string? TitleResourceKey { get; init; }

    public required IconChar Icon { get; init; }

    public required Brush IconBrush { get; init; }

    public required object ViewModel { get; init; }

    public void UpdateTitle(string newTitle)
    {
        if (_title != newTitle)
        {
            _title = newTitle;
            OnPropertyChanged(nameof(Title));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetProperty(ref string field, string value, [CallerMemberName] string? propertyName = null)
    {
        field = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
