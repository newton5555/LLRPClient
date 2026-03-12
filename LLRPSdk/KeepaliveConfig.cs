

using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Helper class used to manage the reader Keep Alive event configuration
  /// </summary>
  public class KeepaliveConfig : INotifyPropertyChanged
  {
    private bool _enabled;
    private uint _periodInMs;

    /// <summary>
    /// Parameter indicating whether the Keep Alive event is enabled or not.
    /// </summary>
    public bool Enabled
    {
      get => this._enabled;
      set => this.SetProperty<bool>(ref this._enabled, value, nameof (Enabled));
    }

    /// <summary>Parameter defining the Keep Alive event interval.</summary>
    public uint PeriodInMs
    {
      get => this._periodInMs;
      set => this.SetProperty<uint>(ref this._periodInMs, value, nameof (PeriodInMs));
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
