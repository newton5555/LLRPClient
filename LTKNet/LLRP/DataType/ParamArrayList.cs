
using System.Collections.Generic;


namespace Org.LLRP.LTK.LLRPV1.DataType
{
  public class ParamArrayList
  {
    private List<IParameter> data;

    public ParamArrayList() => this.data = new List<IParameter>();

    public void Add(IParameter val) => this.data.Add(val);

    public IParameter this[int index]
    {
      get => this.data[index];
      set => this.data[index] = value;
    }

    public int Count => this.data.Count;

    public int Length => this.data.Count;
  }
}
