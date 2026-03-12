using Avalonia.Controls;
using Avalonia.Input;
using LLRPReaderUI_Avalonia.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LLRPReaderUI_Avalonia.Views;

public partial class InventoryView : UserControl
{
    public InventoryView()
    {
        InitializeComponent();
    }

    private async void CopyEpcMenuItem_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await CopySelectedFieldAsync(row => row.Epc);
    }

    private async void CopyAttachedDataMenuItem_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await CopySelectedFieldAsync(row => row.AttachedData);
    }

    private async void CopySelectedRowsMenuItem_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var lines = GetSelectedRows()
            .Select(row =>
                $"{row.ReceiveTime:yyyy-MM-dd HH:mm:ss.fff}\t{row.Epc}\t{row.Antenna}\t{row.ChannelMhz}\t{row.Rssi}\t{row.SeenCount}\t{row.AttachedData}")
            .ToList();

        if (lines.Count == 0)
        {
            return;
        }

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(string.Join(Environment.NewLine, lines));
        }
    }

    private async void InventoryDataGrid_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            return;
        }

        if (e.Key == Key.E)
        {
            await CopySelectedFieldAsync(row => row.Epc);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.D)
        {
            await CopySelectedFieldAsync(row => row.AttachedData);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.C)
        {
            CopySelectedRowsMenuItem_OnClick(sender, null!);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.A)
        {
            InventoryDataGrid.SelectAll();
            e.Handled = true;
        }
    }

    private async System.Threading.Tasks.Task CopySelectedFieldAsync(Func<InventoryTagItemViewModel, string> selector)
    {
        var values = GetSelectedRows()
            .Select(selector)
            .Where(value => !string.IsNullOrWhiteSpace(value) && value != "-")
            .ToList();

        if (values.Count == 0)
        {
            return;
        }

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(string.Join(Environment.NewLine, values));
        }
    }

    private List<InventoryTagItemViewModel> GetSelectedRows()
    {
        return InventoryDataGrid.SelectedItems
            .OfType<InventoryTagItemViewModel>()
            .ToList();
    }
}
