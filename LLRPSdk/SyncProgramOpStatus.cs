
using System.Collections.Generic;

#nullable disable
namespace LLRPSdk
{
  internal class SyncProgramOpStatus
  {
    public ProgramTagMemoryResult Result { get; set; }

    public bool LockEnabled { get; set; }

    public bool VerifyEnabled { get; set; }

    public bool WriteOpComplete { get; set; }

    public bool LockComplete { get; set; }

    public bool VerifyComplete { get; set; }

    public TagData WrittenData { get; set; }

    public string ErrorDescription { get; private set; }

    public bool ErrorOccurred
    {
      get
      {
        if (this.WriteOpComplete && this.Result.WriteResult.Result != WriteResultStatus.Success)
        {
          this.ErrorDescription = "Error while writing to the tag : " + this.Result.WriteResult.Result.ToString();
          return true;
        }
        if (this.VerifyComplete)
        {
          if (this.Result.VerifyResult.Result != ReadResultStatus.Success)
          {
            this.ErrorDescription = "Error while verifying the write : " + this.Result.VerifyResult.Result.ToString();
            return true;
          }
          if (!this.DataMatches())
          {
            this.ErrorDescription = "Data mismatch during verify. Expected : " + this.WrittenData.ToHexWordString() + " Got : " + this.Result.VerifyResult.Data.ToHexWordString();
            return true;
          }
        }
        if (!this.LockComplete || this.Result.LockResult.Result == LockResultStatus.Success)
          return false;
        this.ErrorDescription = "Error while locking tag : " + this.Result.LockResult.Result.ToString();
        return true;
      }
    }

    public bool AllOpsComplete
    {
      get
      {
        int num1 = 1;
        if (this.LockEnabled)
          ++num1;
        if (this.VerifyEnabled)
          ++num1;
        int num2 = 0;
        if (this.WriteOpComplete)
          ++num2;
        if (this.LockComplete)
          ++num2;
        if (this.VerifyComplete)
          ++num2;
        return num2 == num1;
      }
    }

    public SyncProgramOpStatus()
    {
      this.Result = new ProgramTagMemoryResult();
      this.LockComplete = false;
      this.VerifyComplete = false;
      this.VerifyEnabled = false;
      this.LockEnabled = false;
      this.WriteOpComplete = false;
      this.WrittenData = new TagData();
    }

    private bool DataMatches()
    {
      List<ushort> list1 = this.WrittenData.ToList();
      List<ushort> list2 = this.Result.VerifyResult.Data.ToList();
      if (list1.Count != list2.Count)
        return false;
      for (int index = 0; index < list1.Count; ++index)
      {
        if ((int) list1[index] != (int) list2[index])
          return false;
      }
      return true;
    }
  }
}
