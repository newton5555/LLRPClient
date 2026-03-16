namespace LLRPReaderUI_WPF.Models;

using FontAwesome.Sharp;
using System.Windows.Media;

public sealed class NavigationItem
{
    public required string Title { get; init; }

    public required IconChar Icon { get; init; }

    public required Brush IconBrush { get; init; }

    public required object ViewModel { get; init; }
}
