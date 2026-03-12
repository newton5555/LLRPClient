

using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable disable
namespace LLRPSdk
{
    /// <summary>
    /// Class for defining the trigger conditions that will prompt an llrp
    /// reader to automatically start operation. Even when an auto-start trigger
    /// is set, the Reader may be explicitly started using the
    /// <see cref="M:LLRPSdk.LLRPReader.Stop" /> method.
    /// </summary>
    public class AutoStartConfig : INotifyPropertyChanged
  {
    private AutoStartMode _mode;
    private ushort _gpiPortNumber;
    private bool _gpiLevel;
    private uint _firstDelayInMs;
    private uint _periodInMs;
    private ulong _utcTimestamp;

    /// <summary>
    /// Indicates whether and how the Reader should automatically start;
    /// either None, Immediate, Periodic or GpiTrigger.
    /// </summary>
    public AutoStartMode Mode
    {
      get => this._mode;
      set => this.SetProperty<AutoStartMode>(ref this._mode, value, nameof (Mode));
    }

    /// <summary>
    /// When using GpiTrigger mode, this is the port number of a GPI; when
    /// it changes to the GpiLevel value the Reader starts. Possible values
    /// are 1-4. Note that the GPI port must be enabled
    /// </summary>
    public ushort GpiPortNumber
    {
      get => this._gpiPortNumber;
      set => this.SetProperty<ushort>(ref this._gpiPortNumber, value, nameof (GpiPortNumber));
    }

    /// <summary>
    /// When using GpiTrigger mode, and when the GPI Port changes to this
    /// level, then the reader starts. True for high, false for low.
    /// </summary>
    public bool GpiLevel
    {
      get => this._gpiLevel;
      set => this.SetProperty<bool>(ref this._gpiLevel, value, nameof (GpiLevel));
    }

    /// <summary>
    /// When using Periodic mode, this specifies a time offset, or how long
    /// the Reader delays before it first starts, either after the UTCTimestamp,
    /// or after receiving the Settings. After that, the Reader will, if stopped,
    /// start according to PeriodInMs. Zero (0) means there is no initial delay.
    /// Possible values are 0 to 1000000000.
    /// </summary>
    public uint FirstDelayInMs
    {
      get => this._firstDelayInMs;
      set => this.SetProperty<uint>(ref this._firstDelayInMs, value, nameof (FirstDelayInMs));
    }

    /// <summary>
    /// When using Periodic mode, this is how often the Reader tries to start
    /// if it is stopped. The period is carefully maintained, and is relative
    /// to the previous periodic start. It has nothing to do with when the
    /// Reader stops. Zero (0) means there is no periodic start, only one
    /// start after the FirstDelayInMs. Possible values are 0 to 1000000000.
    /// </summary>
    public uint PeriodInMs
    {
      get => this._periodInMs;
      set => this.SetProperty<uint>(ref this._periodInMs, value, nameof (PeriodInMs));
    }

    /// <summary>
    /// UTC timestamp establishing the zero time for autostart periodic
    /// mode timers.  If a UTC time is specified, the the first start time
    /// is determined as (UTC time + offset).  Otherwise, the first start
    /// time is determined as (time of message receipt + offset).
    /// Subsequent start times = first start time + k * period (where, k &gt; 0)
    /// </summary>
    public ulong UtcTimestamp
    {
      get => this._utcTimestamp;
      set => this.SetProperty<ulong>(ref this._utcTimestamp, value, nameof (UtcTimestamp));
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
