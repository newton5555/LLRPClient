using System.Collections.Generic;


namespace Org.LLRP.LTK.LLRPV1.DataType
{
  public class CustomParameterArrayList
  {
    private List<ICustom_Parameter> list;

    public CustomParameterArrayList() => this.list = new List<ICustom_Parameter>();

    public int Length => this.list.Count;

    public int Count => this.list.Count;

    public int Add(ICustom_Parameter value)
    {
      this.list.Add(value);
      return this.list.Count;
    }

    public ICustom_Parameter this[int index]
    {
      get => this.list[index];
      set => this.list[index] = value;
    }
  }
}
