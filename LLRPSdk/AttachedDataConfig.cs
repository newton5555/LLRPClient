using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Configuration for inventory attached data access operation.
  /// </summary>
  public class AttachedDataConfig : INotifyPropertyChanged
  {
    private bool _enabled;
    private MemoryBank _memoryBank;
    private ushort _wordPointer;
    private ushort _wordCount;
    private string _accessPassword;

    /// <summary>
    /// Enable or disable the automatic attached-data access operation.
    /// </summary>
    public bool Enabled
    {
      get => this._enabled;
      set => this.SetProperty<bool>(ref this._enabled, value, nameof (Enabled));
    }

    /// <summary>
    /// Memory bank to read as attached data.
    /// </summary>
    public MemoryBank MemoryBank
    {
      get => this._memoryBank;
      set => this.SetProperty<MemoryBank>(ref this._memoryBank, value, nameof (MemoryBank));
    }

    /// <summary>
    /// Start word pointer for attached-data read.
    /// </summary>
    public ushort WordPointer
    {
      get => this._wordPointer;
      set => this.SetProperty<ushort>(ref this._wordPointer, value, nameof (WordPointer));
    }

    /// <summary>
    /// Word count for attached-data read.
    /// </summary>
    public ushort WordCount
    {
      get => this._wordCount;
      set => this.SetProperty<ushort>(ref this._wordCount, value, nameof (WordCount));
    }

    /// <summary>
    /// Access password in hex string format. Default is 00000000.
    /// </summary>
    public string AccessPassword
    {
      get => this._accessPassword;
      set => this.SetProperty<string>(ref this._accessPassword, string.IsNullOrWhiteSpace(value) ? "00000000" : value, nameof (AccessPassword));
    }

    public AttachedDataConfig()
    {
      this.Enabled = false;
      this.MemoryBank = MemoryBank.Tid;
      this.WordPointer = 0;
      this.WordCount = 6;
      this.AccessPassword = "00000000";
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
