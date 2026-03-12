using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LLRPReaderUI_WPF.Views;

public partial class LogView : UserControl
{
    public LogView()
    {
        InitializeComponent();
        AttachCopySupport(OperationListBox);
        AttachCopySupport(LlrpMessageListBox);
        AttachCopySupport(RawPacketListBox);
    }

    private static void AttachCopySupport(ListBox listBox)
    {
        if (listBox.ContextMenu == null)
        {
            listBox.ContextMenu = new ContextMenu();
        }

        var copyMenuItem = new MenuItem { Header = "复制" };
        copyMenuItem.Click += (_, _) => CopySelectionToClipboard(listBox);
        listBox.ContextMenu.Items.Add(copyMenuItem);

        listBox.PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.C && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                CopySelectionToClipboard(listBox);
                e.Handled = true;
                return;
            }

            if (e.Key == Key.A && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                listBox.SelectAll();
                e.Handled = true;
            }
        };
    }

    private static void CopySelectionToClipboard(ListBox listBox)
    {
        var selectedLines = listBox.SelectedItems.Cast<object>()
            .Select(item => item?.ToString())
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToList();

        if (selectedLines.Count == 0)
        {
            return;
        }

        Clipboard.SetText(string.Join(System.Environment.NewLine, selectedLines));
    }
}
