using System;
using System.Collections;


namespace Org.LLRP.LTK.LLRPV1.DataType
{
  public class AutoGrowingBitArray
  {
    private BitArray inner;
    private int length;
    private int capacity;

    public AutoGrowingBitArray(int capacity)
    {
      this.inner = new BitArray(capacity);
      this.capacity = capacity;
    }

    public int Length
    {
      get => this.length;
      set
      {
        if (value > this.capacity)
        {
          this.capacity = value * 2;
          this.inner.Length = this.capacity;
        }
        this.length = value;
      }
    }

    public bool this[int i]
    {
      get => this.inner[i];
      set
      {
        int length = this.inner.Length;
        if (i >= length)
          this.inner.Length = i * 2;
        if (i >= this.length)
          this.length = i + 1;
        this.inner[i] = value;
      }
    }

    public void CopyTo(Array array, int index)
    {
      for (int index1 = index; index1 < this.length; ++index1)
        array.SetValue((object) this.inner[index1], index1);
    }
  }
}
