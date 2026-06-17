using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LLRPSdk;
using LLRPReaderUI_Avalonia.Messages;
using LLRPReaderUI_Avalonia.Models;
using LLRPReaderUI_Avalonia.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace LLRPReaderUI_Avalonia.ViewModels;

public partial class GpioViewModel : ViewModelBase
{
    private readonly LlrpReader reader;
    private readonly ReaderSettingsStore settingsStore;
    private readonly ReaderStatusStore statusStore;
    private readonly LanguageService _languageService;

    public GpioViewModel(
        LlrpReader reader,
        ReaderSettingsStore settingsStore,
        ReaderStatusStore statusStore,
        LanguageService languageService)
    {
        this.reader = reader;
        this.settingsStore = settingsStore;
        this.statusStore = statusStore;
        _languageService = languageService;
        WeakReferenceMessenger.Default.Register<GpioViewModel, ConnectionStateChangedMessage>(this, static (r, m) =>
        {
            r.OnConnectionStateChanged(m.Value);
        });

        // Set initial state
        OperationResult = _languageService.GetLocalizedString("Common.NotOperated");
    }

    [ObservableProperty]
    private string operationResult = string.Empty;

    [ObservableProperty]
    private ObservableCollection<GpiPortItemViewModel> gpis = new();

    [ObservableProperty]
    private ObservableCollection<GpoPortItemViewModel> gpos = new();

    /// <summary>
    /// Get a localized string with format arguments.
    /// </summary>
    private string GetLocalizedString(string key, params object[] args)
    {
        var format = _languageService.GetLocalizedString(key);
        return args.Length > 0 ? string.Format(format, args) : format;
    }

    [RelayCommand]
    private void QueryGpioSettings()
    {
        try
        {
            if (!reader.IsConnected)
            {
                OperationResult = _languageService.GetLocalizedString("GPIO.ConnectReaderFirst");
                return;
            }

            if (!settingsStore.TryGetSnapshot(out var settings) || settings is null)
            {
                OperationResult = _languageService.GetLocalizedString("GPIO.GetSettingsFirst");
                return;
            }

            if (!statusStore.TryGetSnapshot(out var status) || status is null)
            {
                status = reader.QueryStatus();
                statusStore.Set(status);
            }

            var gpiConfigByPort = settings.Gpis.GpiConfigs
                .GroupBy(x => x.PortNumber)
                .ToDictionary(x => x.Key, x => x.First());
            var gpiStateByPort = status.Gpis
                .Cast<GpiStatus>()
                .ToDictionary(x => x.PortNumber, x => x.State);

            var highText = _languageService.GetLocalizedString("Common.High");
            var lowText = _languageService.GetLocalizedString("Common.Low");
            var unknownText = _languageService.GetLocalizedString("Common.Unknown");

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
                        ? (gpiState ? highText : lowText)
                        : unknownText
                });
            }

            var gpoStateByPort = status.GpoStates
                .Cast<GpoStatus>()
                .ToDictionary(x => x.PortNumber, x => x.State);

            var noResponseText = _languageService.GetLocalizedString("Common.NoResponse");

            Gpos.Clear();
            for (var port = 1; port <= reader.ReaderCapabilities.GpoCount; port++)
            {
                var portNumber = (ushort)port;
                var hasState = gpoStateByPort.TryGetValue(portNumber, out var gpoState);
                Gpos.Add(new GpoPortItemViewModel
                {
                    PortNumber = portNumber,
                    DesiredState = hasState && gpoState,
                    CurrentStateText = hasState ? (gpoState ? highText : lowText) : noResponseText
                });
            }

            OperationResult = _languageService.GetLocalizedString("GPIO.GotConfig");
        }
        catch (Exception ex)
        {
            OperationResult = GetLocalizedString("GPIO.GetFailed", ex.Message);
        }
    }

    [RelayCommand]
    private void SaveGpiSettings()
    {
        try
        {
            if (!reader.IsConnected)
            {
                OperationResult = _languageService.GetLocalizedString("GPIO.ConnectReaderFirst");
                return;
            }

            if (!settingsStore.TryGetSnapshot(out var settings) || settings is null)
            {
                OperationResult = _languageService.GetLocalizedString("GPIO.GetSettingsFirst");
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
            OperationResult = _languageService.GetLocalizedString("GPIO.GpiSaved");
        }
        catch (Exception ex)
        {
            OperationResult = GetLocalizedString("GPIO.GpiSaveFailed", ex.Message);
        }
    }

    [RelayCommand]
    private void ApplyGpoStates()
    {
        try
        {
            if (!reader.IsConnected)
            {
                OperationResult = _languageService.GetLocalizedString("GPIO.ConnectReaderFirst");
                return;
            }

            foreach (var gpo in Gpos)
            {
                reader.SetGpo(gpo.PortNumber, gpo.DesiredState);
            }

            var status = reader.QueryStatus();
            statusStore.Set(status);
            var gpoStateByPort = status.GpoStates
                .Cast<GpoStatus>()
                .ToDictionary(x => x.PortNumber, x => x.State);

            var highText = _languageService.GetLocalizedString("Common.High");
            var lowText = _languageService.GetLocalizedString("Common.Low");
            var noResponseText = _languageService.GetLocalizedString("Common.NoResponse");

            foreach (var gpo in Gpos)
            {
                if (gpoStateByPort.TryGetValue(gpo.PortNumber, out var state))
                {
                    gpo.CurrentStateText = state ? highText : lowText;
                }
                else
                {
                    gpo.CurrentStateText = noResponseText;
                }
            }

            OperationResult = _languageService.GetLocalizedString("GPIO.GpoApplied");
        }
        catch (Exception ex)
        {
            OperationResult = GetLocalizedString("GPIO.GpoApplyFailed", ex.Message);
        }
    }

    public void OnConnectionStateChanged(bool isConnected)
    {
        if (!isConnected)
        {
            Gpis.Clear();
            Gpos.Clear();
            OperationResult = _languageService.GetLocalizedString("GPIO.ConnectReaderFirst");
            return;
        }

        OperationResult = settingsStore.HasValue
            ? _languageService.GetLocalizedString("GPIO.CanReadCache")
            : _languageService.GetLocalizedString("GPIO.GetSettingsFirst");

        if(QueryGpioSettingsCommand.CanExecute(null))
        {
            QueryGpioSettingsCommand.Execute(null);
        }
    }
}

public partial class GpiPortItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private ushort portNumber;

    [ObservableProperty]
    private bool isEnabled;

    [ObservableProperty]
    private string currentStateText = string.Empty;
}

public partial class GpoPortItemViewModel : ViewModelBase
{
    [ObservableProperty]
    private ushort portNumber;

    [ObservableProperty]
    private bool desiredState;

    [ObservableProperty]
    private string currentStateText = string.Empty;
}
