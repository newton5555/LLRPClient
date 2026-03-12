
using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Class for configuring the tag inventory reports returned by the reader.
  /// </summary>
  public class ReportConfig : INotifyPropertyChanged
  {
    private bool _includeAntennaPortNumber;
    private bool _includeChannel;
    private bool _includeFirstSeenTime;
    private bool _includeLastSeenTime;
    private bool _includeSeenCount;
    private bool _includePeakRssi;
    private bool _includePcBits;
    private bool _includeCrc;
    private ReportMode _mode;
    private bool _includeTxPower;
    private bool _includeXPCWords;
    private bool _includeEnhancedIntegra;
    private bool _includeEndpointICVerification;


      



        /// <summary>Include Antenna Port Number in the inventory report</summary>
        public bool IncludeAntennaPortNumber
    {
      get => this._includeAntennaPortNumber;
      set
      {
        this.SetProperty<bool>(ref this._includeAntennaPortNumber, value, nameof (IncludeAntennaPortNumber));
      }
    }

    /// <summary>Include the Channel ID in the report</summary>
    public bool IncludeChannel
    {
      get => this._includeChannel;
      set => this.SetProperty<bool>(ref this._includeChannel, value, nameof (IncludeChannel));
    }

    /// <summary>Include the first seen timestamp of the tag.</summary>
    public bool IncludeFirstSeenTime
    {
      get => this._includeFirstSeenTime;
      set
      {
        this.SetProperty<bool>(ref this._includeFirstSeenTime, value, nameof (IncludeFirstSeenTime));
      }
    }

    /// <summary>Include the last seen timestamp of the tag.</summary>
    public bool IncludeLastSeenTime
    {
      get => this._includeLastSeenTime;
      set
      {
        this.SetProperty<bool>(ref this._includeLastSeenTime, value, nameof (IncludeLastSeenTime));
      }
    }

   

    /// <summary>Include the number of times a tag was seen within that report</summary>
    public bool IncludeSeenCount
    {
      get => this._includeSeenCount;
      set => this.SetProperty<bool>(ref this._includeSeenCount, value, nameof (IncludeSeenCount));
    }

    /// <summary>Include Peak RSSI in each tag report.</summary>
    public bool IncludePeakRssi
    {
      get => this._includePeakRssi;
      set => this.SetProperty<bool>(ref this._includePeakRssi, value, nameof (IncludePeakRssi));
    }

    /// <summary>Include the Protocol Control (PC) bits with each tag read.</summary>
    public bool IncludePcBits
    {
      get => this._includePcBits;
      set => this.SetProperty<bool>(ref this._includePcBits, value, nameof (IncludePcBits));
    }

    /// <summary>Enable the reporting of the CRC bits for the EPC.</summary>
    public bool IncludeCrc
    {
      get => this._includeCrc;
      set => this.SetProperty<bool>(ref this._includeCrc, value, nameof (IncludeCrc));
    }

    /// <summary>
    /// The Mode in which inventory tag reports are generated.
    /// </summary>
    public ReportMode Mode
    {
      get => this._mode;
      set => this.SetProperty<ReportMode>(ref this._mode, value, nameof (Mode));
    }

    /// <summary>Enable the reporting of the TxPower</summary>
    public bool IncludeTxPower
    {
      get => this._includeTxPower;
      set => this.SetProperty<bool>(ref this._includeTxPower, value, nameof (IncludeTxPower));
    }


    /// <summary>
    /// Enable the reporting of XPC Words custom parameter in the tag report data on supported readers
    /// </summary>
    public bool IncludeXPCWords
    {
      get => this._includeXPCWords;
      set => this.SetProperty<bool>(ref this._includeXPCWords, value, nameof (IncludeXPCWords));
    }

    /// <summary>
    /// Enable reporting of Enhanced Integra in the tag report data on supported readers
    /// </summary>
    public bool IncludeEnhancedIntegra
    {
      get => this._includeEnhancedIntegra;
      set
      {
        this.SetProperty<bool>(ref this._includeEnhancedIntegra, value, nameof (IncludeEnhancedIntegra));
      }
    }

    /// <summary>
    /// Enable reporting of Tag Chip Identification in the tag report data on supported readers
    /// </summary>
    public bool IncludeEndpointICVerification
    {
      get => this._includeEndpointICVerification;
      set
      {
        this.SetProperty<bool>(ref this._includeEndpointICVerification, value, nameof (IncludeEndpointICVerification));
      }
    }

    /// <summary>Occurs when a property value changes.</summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>Raises the PropertyChanged event.</summary>
    /// <param name="propertyName"></param>
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
