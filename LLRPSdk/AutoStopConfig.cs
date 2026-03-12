

using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable disable
namespace LLRPSdk
{
    /// <summary>
    /// Class for defining the trigger conditions that will prompt an llrp
    /// reader to automatically stop its current operation. Even when an
    /// auto-stop trigger is set, the Reader may be explicitly stopped using
    /// the <see cref="M:LLRPSdk.LLRPReader.Stop" /> method.
    /// </summary>
    public class AutoStopConfig : INotifyPropertyChanged
  {
    private AutoStopMode _mode;
    private uint _durationInMs;
    private ushort _gpiPortNumber;
    private bool _gpiLevel;
    private uint _timeout;

    /// <summary>
    /// Indicates whether and how the Reader should automatically stop;
    /// either None, Duration or GpiTrigger.
    /// </summary>
    public AutoStopMode Mode
    {
      get => this._mode;
      set => this.SetProperty<AutoStopMode>(ref this._mode, value, nameof (Mode));
    }

    /// <summary>
    /// When using Duration mode, this is period for which the Reader
    /// singulates.
    /// </summary>
    public uint DurationInMs
    {
      get => this._durationInMs;
      set => this.SetProperty<uint>(ref this._durationInMs, value, nameof (DurationInMs));
    }

    /// <summary>GPI port to monitor for a stop event.</summary>
    public ushort GpiPortNumber
    {
      get => this._gpiPortNumber;
      set => this.SetProperty<ushort>(ref this._gpiPortNumber, value, nameof (GpiPortNumber));
    }

    /// <summary>
    /// When using GpiTrigger mode, and when the GPI Port changes to this
    /// level, then the reader stops. True for high, false for low.
    /// </summary>
    public bool GpiLevel
    {
      get => this._gpiLevel;
      set => this.SetProperty<bool>(ref this._gpiLevel, value, nameof (GpiLevel));
    }

    /// <summary>
    /// This is the longest the Reader will singulate even if the GPI does
    /// not change. A value of 0 means there is no time limit.
    /// </summary>
    public uint Timeout
    {
      get => this._timeout;
      set => this.SetProperty<uint>(ref this._timeout, value, nameof (Timeout));
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
