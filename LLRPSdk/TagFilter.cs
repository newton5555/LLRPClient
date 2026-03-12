
using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Class for containing the settings for a single inventory filter.
  /// </summary>
  public class TagFilter : INotifyPropertyChanged
  {
    private string _tagMask;
    private ushort _bitPointer;
    private int _bitCount;
    private MemoryBank _memoryBank;
    private TagFilterOp _filterOp;

    /// <summary>
    /// The tag mask defines the bit pattern that the filter must match on.
    /// The mask should be expressed as a hex string.
    /// </summary>
    public string TagMask
    {
      get => this._tagMask;
      set => this.SetProperty<string>(ref this._tagMask, value, nameof (TagMask));
    }

    /// <summary>
    /// The bit offset in the specified memory bank at which the tag mask
    /// begins. It is important to note that this is a bit offset and need
    /// not be word or even byte-aligned.
    /// </summary>
    public ushort BitPointer
    {
      get => this._bitPointer;
      set => this.SetProperty<ushort>(ref this._bitPointer, value, nameof (BitPointer));
    }

    /// <summary>
    /// The length of the tag mask in bits.
    /// If no length is specified, the entire mask is used.
    /// </summary>
    public int BitCount
    {
      get => this._bitCount;
      set => this.SetProperty<int>(ref this._bitCount, value, nameof (BitCount));
    }

    /// <summary>
    /// The memory bank on which the filter is applied. Filters may be
    /// configured to search for content in the Epc, Tid, and User memory
    /// banks. Filters may not match against the Reserved memory bank.
    /// </summary>
    public MemoryBank MemoryBank
    {
      get => this._memoryBank;
      set => this.SetProperty<MemoryBank>(ref this._memoryBank, value, nameof (MemoryBank));
    }

    /// <summary />
    public TagFilterOp FilterOp
    {
      get => this._filterOp;
      set => this.SetProperty<TagFilterOp>(ref this._filterOp, value, nameof (FilterOp));
    }

    /// <summary>
    /// Initializes a new instance of the TagFilter class, initializing
    /// the memory bank to EPC.
    /// </summary>
    public TagFilter() => this.FilterOp = TagFilterOp.Match;

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
