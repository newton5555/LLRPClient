using Avalonia.Controls;
using LLRPReaderUI_Avalonia.ViewModels;
using System;
using System.Linq;
using System.Text;

namespace LLRPReaderUI_Avalonia.Views;

public partial class InventoryView : UserControl
{
    private DataGrid? _dataGrid;
    private DataGridColumn? _antennaColumn;
    private DataGridColumn? _channelColumn;
    private DataGridColumn? _rssiColumn;
    private DataGridColumn? _seenCountColumn;
    private DataGridColumn? _pcColumn;
    private DataGridColumn? _crcColumn;
    private DataGridColumn? _firstSeenColumn;
    private DataGridColumn? _lastSeenColumn;
    private DataGridColumn? _attachedDataColumn;

    public InventoryView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Find the DataGrid and columns after the control is loaded
        _dataGrid = this.FindControl<DataGrid>("InventoryDataGrid");
        if (_dataGrid != null)
        {
            // Find columns by header name
            foreach (var column in _dataGrid.Columns)
            {
                var header = column.Header?.ToString();
                switch (header)
                {
                    case var h when h?.Contains("Antenna") == true || h?.Contains("Ant") == true:
                        _antennaColumn = column;
                        break;
                    case var h when h?.Contains("Frequency") == true || h?.Contains("频") == true:
                        _channelColumn = column;
                        break;
                    case var h when h?.Contains("RSSI") == true:
                        _rssiColumn = column;
                        break;
                    case var h when h?.Contains("Count") == true || h?.Contains("计数") == true:
                        _seenCountColumn = column;
                        break;
                    case "PC":
                        _pcColumn = column;
                        break;
                    case "CRC":
                        _crcColumn = column;
                        break;
                    case var h when h?.Contains("FirstTimestamp") == true:
                        _firstSeenColumn = column;
                        break;
                    case var h when h?.Contains("LastTimestamp") == true:
                        _lastSeenColumn = column;
                        break;
                    case var h when h?.Contains("AttachedData") == true || h?.Contains("附加") == true:
                        _attachedDataColumn = column;
                        break;
                }
            }
        }

        if (DataContext is InventoryViewModel viewModel)
        {
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdateColumnVisibility(viewModel);
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is InventoryViewModel viewModel)
        {
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdateColumnVisibility(viewModel);
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (DataContext is InventoryViewModel viewModel)
        {
            UpdateColumnVisibility(viewModel, e.PropertyName);
        }
    }

    private void UpdateColumnVisibility(InventoryViewModel viewModel, string? propertyName = null)
    {
        // Update column visibility based on ViewModel properties
        if (propertyName == null || propertyName == nameof(InventoryViewModel.ShowAntennaPortNumberColumn))
            _antennaColumn?.SetCurrentValue(DataGridColumn.IsVisibleProperty, viewModel.ShowAntennaPortNumberColumn);

        if (propertyName == null || propertyName == nameof(InventoryViewModel.ShowChannelColumn))
            _channelColumn?.SetCurrentValue(DataGridColumn.IsVisibleProperty, viewModel.ShowChannelColumn);

        if (propertyName == null || propertyName == nameof(InventoryViewModel.ShowPeakRssiColumn))
            _rssiColumn?.SetCurrentValue(DataGridColumn.IsVisibleProperty, viewModel.ShowPeakRssiColumn);

        if (propertyName == null || propertyName == nameof(InventoryViewModel.ShowSeenCountColumn))
            _seenCountColumn?.SetCurrentValue(DataGridColumn.IsVisibleProperty, viewModel.ShowSeenCountColumn);

        if (propertyName == null || propertyName == nameof(InventoryViewModel.ShowPcColumn))
            _pcColumn?.SetCurrentValue(DataGridColumn.IsVisibleProperty, viewModel.ShowPcColumn);

        if (propertyName == null || propertyName == nameof(InventoryViewModel.ShowCrcColumn))
            _crcColumn?.SetCurrentValue(DataGridColumn.IsVisibleProperty, viewModel.ShowCrcColumn);

        if (propertyName == null || propertyName == nameof(InventoryViewModel.ShowFirstSeenTimestampUtcColumn))
            _firstSeenColumn?.SetCurrentValue(DataGridColumn.IsVisibleProperty, viewModel.ShowFirstSeenTimestampUtcColumn);

        if (propertyName == null || propertyName == nameof(InventoryViewModel.ShowLastSeenTimestampUtcColumn))
            _lastSeenColumn?.SetCurrentValue(DataGridColumn.IsVisibleProperty, viewModel.ShowLastSeenTimestampUtcColumn);

        if (propertyName == null || propertyName == nameof(InventoryViewModel.AttachedDataEnabled))
            _attachedDataColumn?.SetCurrentValue(DataGridColumn.IsVisibleProperty, viewModel.AttachedDataEnabled);
    }

    private void CopyEpcMenuItem_OnClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_dataGrid?.SelectedItem is InventoryTagItemViewModel item)
        {
            CopyToClipboard(item.Epc);
        }
    }

    private void CopyAttachedDataMenuItem_OnClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_dataGrid?.SelectedItem is InventoryTagItemViewModel item)
        {
            CopyToClipboard(item.AttachedData);
        }
    }

    private void CopySelectedRowsMenuItem_OnClick(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_dataGrid == null) return;

        var selectedItems = _dataGrid.SelectedItems
            .OfType<InventoryTagItemViewModel>()
            .ToList();

        if (selectedItems.Count == 0)
            return;

        var sb = new StringBuilder();
        sb.AppendLine("ReceiveTime\tEPC\tAntenna\tFrequency\tRSSI\tCount\tPC\tCRC\tFirstTimestamp\tLastTimestamp\tAttachedData");

        foreach (var item in selectedItems)
        {
            sb.AppendLine($"{item.ReceiveTime:MM/dd HH:mm:ss.fff}\t{item.Epc}\t{item.Antenna}\t{item.ChannelMhz}\t{item.Rssi}\t{item.SeenCount}\t{item.Pc}\t{item.Crc}\t{item.FirstSeenTimestampUtc}\t{item.LastSeenTimestampUtc}\t{item.AttachedData}");
        }

        CopyToClipboard(sb.ToString());
    }

    private async void CopyToClipboard(string text)
    {
        if (TopLevel.GetTopLevel(this) is { } topLevel && topLevel.Clipboard is { } clipboard)
        {
            await clipboard.SetTextAsync(text);
        }
    }
}
