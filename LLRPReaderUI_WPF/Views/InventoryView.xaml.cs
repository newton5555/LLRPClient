using LLRPReaderUI_WPF.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LLRPReaderUI_WPF.Views;

public partial class InventoryView : UserControl
{
    public InventoryView()
    {
        InitializeComponent();
    }

    private void CopyEpcMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        CopySelectedField(row => row.Epc);
    }

    private void CopyAttachedDataMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        CopySelectedField(row => row.AttachedData);
    }

    private void CopySelectedRowsMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        var lines = GetSelectedRows()
            .Select(row =>
                $"{row.ReceiveTime:yyyy-MM-dd HH:mm:ss.fff}\t{row.Epc}\t{row.Antenna}\t{row.ChannelMhz}\t{row.Rssi}\t{row.SeenCount}\t{row.AttachedData}")
            .ToList();

        if (lines.Count == 0)
        {
            return;
        }

        Clipboard.SetText(string.Join(System.Environment.NewLine, lines));
    }

    private void InventoryDataGrid_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            return;
        }

        if (e.Key == Key.E)
        {
            CopySelectedField(row => row.Epc);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.D)
        {
            CopySelectedField(row => row.AttachedData);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.C)
        {
            CopySelectedRowsMenuItem_OnClick(sender, new RoutedEventArgs());
            e.Handled = true;
            return;
        }

        if (e.Key == Key.A)
        {
            InventoryDataGrid.SelectAll();
            e.Handled = true;
        }
    }

    private void CopySelectedField(Func<InventoryTagItemViewModel, string> selector)
    {
        var values = GetSelectedRows()
            .Select(selector)
            .Where(value => !string.IsNullOrWhiteSpace(value) && value != "-")
            .ToList();

        if (values.Count == 0)
        {
            return;
        }

        Clipboard.SetText(string.Join(System.Environment.NewLine, values));
    }

    private List<InventoryTagItemViewModel> GetSelectedRows()
    {
        return InventoryDataGrid.SelectedItems
            .OfType<InventoryTagItemViewModel>()
            .ToList();
    }
}
