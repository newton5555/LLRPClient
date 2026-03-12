using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LLRPSdk;
using LLRPReaderUI_Avalonia.Messages;
using LLRPReaderUI_Avalonia.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System;

namespace LLRPReaderUI_Avalonia.ViewModels;

public partial class GpioViewModel : ViewModelBase
{
    private readonly LlrpReader reader;
    private readonly ReaderSettingsStore settingsStore;

    public GpioViewModel(LlrpReader reader, ReaderSettingsStore settingsStore)
    {
        this.reader = reader;
        this.settingsStore = settingsStore;
        WeakReferenceMessenger.Default.Register<GpioViewModel, ConnectionStateChangedMessage>(this, static (r, m) =>
        {
            r.OnConnectionStateChanged(m.Value);
        });
    }

    [ObservableProperty]
    private string operationResult = "未操作";

    [ObservableProperty]
    private ObservableCollection<GpiPortItemViewModel> gpis = new();

    [ObservableProperty]
    private ObservableCollection<GpoPortItemViewModel> gpos = new();

    [RelayCommand]
    private void QueryGpioSettings()
    {
        try
        {
            if (!reader.IsConnected)
            {
                OperationResult = "请先连接读写器";
                return;
            }

            if (!settingsStore.TryGetSnapshot(out var settings) || settings is null)
            {
                OperationResult = "请先在参数配置页点击获取参数";
                return;
            }

            var status = reader.QueryStatus();

            var gpiConfigByPort = settings.Gpis.GpiConfigs
                .GroupBy(x => x.PortNumber)
                .ToDictionary(x => x.Key, x => x.First());
            var gpiStateByPort = status.Gpis
                .Cast<GpiStatus>()
                .ToDictionary(x => x.PortNumber, x => x.State);

            Gpis.Clear();
            for (var port = 1; port <= reader.ReaderCapabilities.GpiCount; port++)
            {
                var portNumber = (ushort)port;
                gpiConfigByPort.TryGetValue(portNumber, out var gpiConfig);
                gpiStateByPort.TryGetValue(portNumber, out var gpiState);

                Gpis.Add(new GpiPortItemViewModel
                {
                    PortNumber = portNumber,
                    IsEnabled = gpiConfig?.IsEnabled ?? false,
                    CurrentStateText = gpiStateByPort.ContainsKey(portNumber)
                        ? (gpiState ? "高" : "低")
                        : "未知"
                });
            }

            var gpoStateByPort = status.GpoStates
                .Cast<GpoStatus>()
                .ToDictionary(x => x.PortNumber, x => x.State);

            Gpos.Clear();
            for (var port = 1; port <= reader.ReaderCapabilities.GpoCount; port++)
            {
                var portNumber = (ushort)port;
                var hasState = gpoStateByPort.TryGetValue(portNumber, out var gpoState);
                Gpos.Add(new GpoPortItemViewModel
                {
                    PortNumber = portNumber,
                    DesiredState = hasState && gpoState,
                    CurrentStateText = hasState ? (gpoState ? "高" : "低") : "设备未返回"
                });
            }

            OperationResult = "已获取 GPIO 配置与状态";
        }
        catch (Exception ex)
        {
            OperationResult = $"获取失败：{ex.Message}";
        }
    }

    [RelayCommand]
    private void SaveGpiSettings()
    {
        try
        {
            if (!reader.IsConnected)
            {
                OperationResult = "请先连接读写器";
                return;
            }

            if (!settingsStore.TryGetSnapshot(out var settings) || settings is null)
            {
                OperationResult = "请先在参数配置页点击获取参数";
                return;
            }

            var gpiConfigs = settings.Gpis.GpiConfigs;
            foreach (var gpiItem in Gpis)
            {
                var index = gpiItem.PortNumber - 1;
                if (index < 0 || index >= gpiConfigs.Count)
                {
                    continue;
                }

                var gpiConfig = gpiConfigs[index];
                gpiConfig.PortNumber = gpiItem.PortNumber;
                gpiConfig.IsEnabled = gpiItem.IsEnabled;
            }

            reader.ApplySettings(settings);
            settingsStore.Set(settings);
            OperationResult = "GPI 配置已下发";
        }
        catch (Exception ex)
        {
            OperationResult = $"下发失败：{ex.Message}";
        }
    }

    [RelayCommand]
    private void ApplyGpoStates()
    {
        try
        {
            if (!reader.IsConnected)
            {
                OperationResult = "请先连接读写器";
                return;
            }

            foreach (var gpo in Gpos)
            {
                reader.SetGpo(gpo.PortNumber, gpo.DesiredState);
            }

            var status = reader.QueryStatus();
            var gpoStateByPort = status.GpoStates
                .Cast<GpoStatus>()
                .ToDictionary(x => x.PortNumber, x => x.State);
            foreach (var gpo in Gpos)
            {
                if (gpoStateByPort.TryGetValue(gpo.PortNumber, out var state))
                {
                    gpo.CurrentStateText = state ? "高" : "低";
                }
                else
                {
                    gpo.CurrentStateText = "设备未返回";
                }
            }

            OperationResult = "GPO 输出已下发";
        }
        catch (Exception ex)
        {
            OperationResult = $"下发失败：{ex.Message}";
        }
    }

    public void OnConnectionStateChanged(bool isConnected)
    {
        if (!isConnected)
        {
            Gpis.Clear();
            Gpos.Clear();
            OperationResult = "请先连接读写器";
            return;
        }

        OperationResult = settingsStore.HasValue ? "可读取缓存参数" : "请先在参数配置页点击获取参数";
    }
}

public partial class GpiPortItemViewModel : ObservableObject
{
    [ObservableProperty]
    private ushort portNumber;

    [ObservableProperty]
    private bool isEnabled;

    [ObservableProperty]
    private string currentStateText = "未知";
}

public partial class GpoPortItemViewModel : ObservableObject
{
    [ObservableProperty]
    private ushort portNumber;

    [ObservableProperty]
    private bool desiredState;

    [ObservableProperty]
    private string currentStateText = "设备未返回";
}


