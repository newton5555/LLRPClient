namespace LLRPReaderUI_WPF.Models;

public sealed class NavigationItem
{
    public required string Title { get; init; }

    public required string Glyph { get; init; }

    public required object ViewModel { get; init; }
}
