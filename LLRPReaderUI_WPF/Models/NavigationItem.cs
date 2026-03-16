namespace LLRPReaderUI_WPF.Models;

using FontAwesome.Sharp;

public sealed class NavigationItem
{
    public required string Title { get; init; }

    public required IconChar Icon { get; init; }

    public required object ViewModel { get; init; }
}
