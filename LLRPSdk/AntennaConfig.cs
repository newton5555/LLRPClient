

using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Class used to define the configuration settings for an
  /// individual antenna port.
  /// </summary>
  public class AntennaConfig : INotifyPropertyChanged
  {
    private ushort _PortNumber;
    private bool _isEnabled;
    private string _portName;
    private double _RxSensitivityinDbm;
    private double _TxPowerinDbm;
    private bool _MaxRxSensitivity;
    private bool _MaxTxPower;
    private double _ReaderActualMaxSensitivityCapability;
    private double _ReaderActualMaxTxPowerCapability;

    /// <summary>Default Constructor</summary>
    public AntennaConfig()
    {
    }

    /// <summary>Constructor for a new antenna configuration object.</summary>
    /// <param name="NewPortNumber">Antenna port to configure. A value of 0 means
    /// configuration is applied to all antennas.</param>
    public AntennaConfig(ushort NewPortNumber)
    {
      this.PortNumber = NewPortNumber;
      this.PortName = string.Format("Antenna Port {0}", (object) this.PortNumber);
      this.IsEnabled = true;
      this.MaxRxSensitivity = true;
      this.MaxTxPower = true;
    }

    /// <summary>If set to true, the reader will try to inventory tags on that port</summary>
    public bool IsEnabled
    {
      get => this._isEnabled;
      set => this.SetProperty<bool>(ref this._isEnabled, value, nameof (IsEnabled));
    }

    /// <summary>
    /// Specifies the antenna port number (starting at port 1) to configure.
    /// </summary>
    /// <exception cref="T:LLRPSdk.LLRPSdkException">
    /// Thrown when PortNumber is set to 0;
    /// </exception>
    public ushort PortNumber
    {
      get => this._PortNumber;
      set
      {
        if (value == (ushort) 0)
          throw new LLRPSdkException("GPO port number 0 is illegal.");
        this.SetProperty<ushort>(ref this._PortNumber, value, nameof (PortNumber));
      }
    }

    /// <summary>A string that the user can name the antenna port. Example: Door Antenna</summary>
    public string PortName
    {
      get => this._portName;
      set => this.SetProperty<string>(ref this._portName, value, nameof (PortName));
    }

    /// <summary>
    /// Defines the antenna transmit power in dBm (e.g. 24.0 dBm).
    /// </summary>
    public double TxPowerInDbm
    {
      get => this._TxPowerinDbm;
      set
      {
        if (!this.SetProperty<double>(ref this._TxPowerinDbm, value, nameof (TxPowerInDbm)))
          return;
        this.MaxTxPower = value == 0.0 || value == this.ReaderMaxTxPowerCapability;
      }
    }

    /// <summary>
    /// Defines the antenna receive sensitivity (AKA receive sensitivity filter)
    /// in dBm (e.g. -55 dBm).
    /// <remarks>
    /// If we have already communicated with the reader, and we know it's MaxRxSensitivity
    /// dBm value (e.g. -80 for Speedway based products) then if the value being set is equal
    /// to that value we will set the related property, MaxRxSensitivity, to true. The developer
    /// is cautioned to understand that this property can in turn set another property and the
    /// binding implications thereof.
    /// </remarks>
    /// </summary>
    public double RxSensitivityInDbm
    {
      get => this._RxSensitivityinDbm;
      set
      {
        if (!this.SetProperty<double>(ref this._RxSensitivityinDbm, value, nameof (RxSensitivityInDbm)) || value == 0.0)
          return;
        this.MaxRxSensitivity = this._ReaderActualMaxSensitivityCapability == value;
      }
    }

    /// <summary>
    /// Specifies the readers's actual max sensitivity, if known
    /// <remarks>
    /// This property is only set when a reader connection exists; and this property is
    /// intended for use only by classes in the OctaneSDK dot net assembly itself as a helper.
    /// </remarks>
    /// </summary>
    protected internal double ReaderMaxSensitivityCapability
    {
      get => this._ReaderActualMaxSensitivityCapability;
      set
      {
        this.SetProperty<double>(ref this._ReaderActualMaxSensitivityCapability, value, nameof (ReaderMaxSensitivityCapability));
      }
    }

    /// <summary>
    /// Specifies the readers's actual max transmit power, if known
    /// <remarks>
    /// This property is only set when a reader connection exists; and this property is
    /// intended for use only by classes in the OctaneSDK dot net assembly itself as a helper.
    /// </remarks>
    /// </summary>
    protected internal double ReaderMaxTxPowerCapability
    {
      get => this._ReaderActualMaxTxPowerCapability;
      set
      {
        this.SetProperty<double>(ref this._ReaderActualMaxTxPowerCapability, value, nameof (ReaderMaxTxPowerCapability));
      }
    }

    /// <summary>
    /// Specifies that the maximum receive sensitivity is to be used.
    /// </summary>
    public bool MaxRxSensitivity
    {
      get => this._MaxRxSensitivity;
      set
      {
        if (!this.SetProperty<bool>(ref this._MaxRxSensitivity, value, nameof (MaxRxSensitivity)) || !value)
          return;
        this.RxSensitivityInDbm = this.ReaderMaxSensitivityCapability;
      }
    }

    /// <summary>
    /// Specifies that the maximum antenna transmit power setting should
    /// be used.
    /// </summary>
    public bool MaxTxPower
    {
      get => this._MaxTxPower;
      set
      {
        if (!this.SetProperty<bool>(ref this._MaxTxPower, value, nameof (MaxTxPower)) || !value)
          return;
        this.TxPowerInDbm = this.ReaderMaxTxPowerCapability;
      }
    }

    /// <summary>Occurs when a property value changes.</summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>Raises the PropertyChanged event.</summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Sets a property, raising the PropertyChanged event if the value of the property changes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="storage"></param>
    /// <param name="value"></param>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
      if (object.Equals((object) storage, (object) value))
        return false;
      storage = value;
      this.OnPropertyChanged(propertyName);
      return true;
    }
  }
}
