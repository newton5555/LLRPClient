
#nullable disable
namespace LLRPSdk
{
  /// <summary>Class used to specify a tag lock operation.</summary>
  public class TagLockOp : TagOp
  {
    private TagData _AccessPassword = new TagData();

    /// <summary>Constructor for TagLockOp class</summary>
    public TagLockOp()
    {
      this.KillPasswordLockType = TagLockState.None;
      this.AccessPasswordLockType = TagLockState.None;
      this.EpcLockType = TagLockState.None;
      this.TidLockType = TagLockState.None;
      this.UserLockType = TagLockState.None;
    }

    /// <summary>
    /// Tag access password.  Required only if an access password has been set
    /// for the target memory bank.
    /// </summary>
    public TagData AccessPassword
    {
      get => this._AccessPassword;
      set
      {
        if (value == null)
          return;
        this._AccessPassword = value;
      }
    }

    /// <summary>Lock operation to perform on the Kill Password.</summary>
    public TagLockState KillPasswordLockType { get; set; }

    /// <summary>Lock operation to perform on the Access Password.</summary>
    public TagLockState AccessPasswordLockType { get; set; }

    /// <summary>Lock operation to perform on the EPC memory bank.</summary>
    public TagLockState EpcLockType { get; set; }

    /// <summary>Lock operation to perform on the TID memory bank.</summary>
    public TagLockState TidLockType { get; set; }

    /// <summary>Lock operation to perform on the User memory bank.</summary>
    public TagLockState UserLockType { get; set; }

    /// <summary>The number of locks specified in this operation.</summary>
    public ushort LockCount
    {
      get
      {
        ushort lockCount = 0;
        if (this.KillPasswordLockType != TagLockState.None)
          ++lockCount;
        if (this.AccessPasswordLockType != TagLockState.None)
          ++lockCount;
        if (this.EpcLockType != TagLockState.None)
          ++lockCount;
        if (this.TidLockType != TagLockState.None)
          ++lockCount;
        if (this.UserLockType != TagLockState.None)
          ++lockCount;
        return lockCount;
      }
    }
  }
}
