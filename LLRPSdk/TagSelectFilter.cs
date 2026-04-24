
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Org.LLRP.LTK.LLRPV1;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Class for containing a tag select for a single inventory filter
  /// </summary>
  public class TagSelectFilter : INotifyPropertyChanged
  {
    private string _tagMask;
    private ushort _bitPointer;
    private int _bitCount;
    private MemoryBank _memoryBank;
    private StateUnawareAction _matchAction;
    private StateUnawareAction _nonMatchAction;
    private bool _useStateAwareAction;
    private ENUM_C1G2StateAwareTarget _stateAwareTarget = ENUM_C1G2StateAwareTarget.SL;
    private ENUM_C1G2StateAwareAction _stateAwareAction = ENUM_C1G2StateAwareAction.AssertSLOrA_DeassertSLOrB;

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
    /// The length of the mask in bits.
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

    /// <summary>The action to perform when the tag filter matches.</summary>
    public StateUnawareAction MatchAction
    {
      get => this._matchAction;
      set
      {
        this.SetProperty<StateUnawareAction>(ref this._matchAction, value, nameof (MatchAction));
      }
    }

    /// <summary>
    /// The action to perform when the tag filter does not match.
    /// </summary>
    public StateUnawareAction NonMatchAction
    {
      get => this._nonMatchAction;
      set
      {
        this.SetProperty<StateUnawareAction>(ref this._nonMatchAction, value, nameof (NonMatchAction));
      }
    }

    /// <summary>
    /// Enables using C1G2TagInventoryStateAwareFilterAction for this filter.
    /// </summary>
    public bool UseStateAwareAction
    {
      get => this._useStateAwareAction;
      set => this.SetProperty<bool>(ref this._useStateAwareAction, value, nameof(UseStateAwareAction));
    }

    /// <summary>
    /// State-aware target (SL/S0/S1/S2/S3) used when UseStateAwareAction is true.
    /// </summary>
    public ENUM_C1G2StateAwareTarget StateAwareTarget
    {
      get => this._stateAwareTarget;
      set => this.SetProperty<ENUM_C1G2StateAwareTarget>(ref this._stateAwareTarget, value, nameof(StateAwareTarget));
    }

    /// <summary>
    /// State-aware action used when UseStateAwareAction is true.
    /// </summary>
    public ENUM_C1G2StateAwareAction StateAwareAction
    {
      get => this._stateAwareAction;
      set => this.SetProperty<ENUM_C1G2StateAwareAction>(ref this._stateAwareAction, value, nameof(StateAwareAction));
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
