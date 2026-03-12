
#nullable disable
namespace LLRPSdk
{
  internal class SyncReadOpStatus
  {
    public ReadTagMemoryResult Result { get; set; }

    public bool ReadOpComplete { get; set; }

    public string ErrorDescription { get; private set; }

    public bool ErrorOccurred
    {
      get
      {
        if (!this.ReadOpComplete || this.Result.ReadResult.Result == ReadResultStatus.Success)
          return false;
        this.ErrorDescription = "Error while reading from the tag : " + this.Result.ReadResult.Result.ToString();
        return true;
      }
    }

    public bool AllOpsComplete => this.ReadOpComplete;

    public SyncReadOpStatus()
    {
      this.Result = new ReadTagMemoryResult();
      this.ReadOpComplete = false;
    }
  }
}
