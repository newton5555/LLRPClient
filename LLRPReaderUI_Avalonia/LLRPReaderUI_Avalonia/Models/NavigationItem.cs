namespace LLRPReaderUI_Avalonia.Models;

using Material.Icons;

public sealed class NavigationItem
{
    public required string Title { get; init; }

    public required MaterialIconKind IconKind { get; init; }

    public required object ViewModel { get; init; }
}

