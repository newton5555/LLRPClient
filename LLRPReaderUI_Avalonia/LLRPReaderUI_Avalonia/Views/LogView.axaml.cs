using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Linq;

namespace LLRPReaderUI_Avalonia.Views;

public partial class LogView : UserControl
{
    public LogView()
    {
        InitializeComponent();
        AttachCopySupport(OperationListBox);
        AttachCopySupport(LlrpMessageListBox);
        AttachCopySupport(RawPacketListBox);
    }

    private void AttachCopySupport(ListBox listBox)
    {
        var menu = new ContextMenu();
        var copyMenuItem = new MenuItem { Header = "复制" };
        copyMenuItem.Click += async (_, _) => await CopySelectionToClipboardAsync(listBox);
        menu.ItemsSource = new[] { copyMenuItem };
        listBox.ContextMenu = menu;

        listBox.KeyDown += async (_, e) =>
        {
            if (e.Key == Key.C && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                await CopySelectionToClipboardAsync(listBox);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.A && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                listBox.SelectAll();
                e.Handled = true;
            }
        };
    }

    private async System.Threading.Tasks.Task CopySelectionToClipboardAsync(ListBox listBox)
    {
        var selectedLines = listBox.SelectedItems.Cast<object>()
            .Select(item => item?.ToString())
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToList();

        if (selectedLines.Count == 0)
        {
            return;
        }

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is not null)
        {
            await clipboard.SetTextAsync(string.Join(Environment.NewLine, selectedLines));
        }
    }
}
