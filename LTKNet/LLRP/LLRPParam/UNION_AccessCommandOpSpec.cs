// Decompiled with JetBrains decompiler
// Type: Org.LLRP.LTK.LLRPV1.UNION_AccessCommandOpSpec
// Assembly: LLRP, Version=12.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A26E050E-ED10-41E4-B0F3-A331CE1B9C2E
// Assembly location: F:\Projects\LLRP\参考OctaneSdk\LLRPSdk\lib\LLRP.dll

using Org.LLRP.LTK.LLRPV1.DataType;


namespace Org.LLRP.LTK.LLRPV1
{
  public class UNION_AccessCommandOpSpec : ParamArrayList
  {
    public bool AddCustomParameter(ICustom_Parameter param)
    {
      if (param is IAccessCommandOpSpec_Custom_Param)
      {
        this.Add((IParameter) param);
        return true;
      }
      if (!(param.GetType() == typeof (PARAM_Custom)))
        return false;
      this.Add((IParameter) param);
      return true;
    }
  }
}
