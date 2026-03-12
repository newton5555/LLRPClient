using System;
using System.Collections;
using System.Reflection;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class CustomParamDecodeFactory
  {
    public static Hashtable vendorExtensionIDTypeHash;
    public static Hashtable vendorExtensionNameTypeHash;
    public static Hashtable vendorExtensionAssemblyHash;

    public static void LoadVendorExtensionAssembly(Assembly asm)
    {
      if (CustomParamDecodeFactory.vendorExtensionIDTypeHash == null)
        CustomParamDecodeFactory.vendorExtensionIDTypeHash = new Hashtable();
      if (CustomParamDecodeFactory.vendorExtensionNameTypeHash == null)
        CustomParamDecodeFactory.vendorExtensionNameTypeHash = new Hashtable();
      if (CustomParamDecodeFactory.vendorExtensionAssemblyHash == null)
        CustomParamDecodeFactory.vendorExtensionAssemblyHash = new Hashtable();
      string name = asm.GetName().Name;
      if (CustomParamDecodeFactory.vendorExtensionAssemblyHash.ContainsKey((object) name))
        return;
      CustomParamDecodeFactory.vendorExtensionAssemblyHash.Add((object) name, (object) asm);
      try
      {
        foreach (Type type in asm.GetTypes())
        {
          if (!(type.BaseType != typeof (PARAM_Custom)))
          {
            string typeName = type.Namespace + "." + type.Name;
            PARAM_Custom instance = (PARAM_Custom) asm.CreateInstance(typeName);
            uint num = instance.VendorID;
            string str1 = num.ToString();
            num = instance.SubType;
            string str2 = num.ToString();
            string key = str1 + "-" + str2;
            if (!CustomParamDecodeFactory.vendorExtensionIDTypeHash.ContainsKey((object) key))
              CustomParamDecodeFactory.vendorExtensionIDTypeHash.Add((object) key, (object) type);
            if (!CustomParamDecodeFactory.vendorExtensionNameTypeHash.ContainsKey((object) type.Name))
              CustomParamDecodeFactory.vendorExtensionNameTypeHash.Add((object) type.Name, (object) type);
          }
        }
      }
      catch
      {
        Console.WriteLine("LVEA failed", (object) asm);
      }
    }

    public static ICustom_Parameter DecodeCustomParameter(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (ICustom_Parameter) null;
      int num1 = cursor;
      PARAM_Custom paramCustom = PARAM_Custom.FromBitArray(ref bit_array, ref cursor, length);
      if (paramCustom == null)
        return (ICustom_Parameter) null;
      uint num2 = paramCustom.VendorID;
      string str1 = num2.ToString();
      num2 = paramCustom.SubType;
      string str2 = num2.ToString();
      string key = str1 + "-" + str2;
      if (CustomParamDecodeFactory.vendorExtensionIDTypeHash != null)
      {
        int num3 = cursor;
        try
        {
          MethodInfo method = ((Type) CustomParamDecodeFactory.vendorExtensionIDTypeHash[(object) key]).GetMethod("FromBitArray");
          if (method == (MethodInfo) null)
            return (ICustom_Parameter) null;
          cursor = num1;
          object[] parameters = new object[3]
          {
            (object) bit_array,
            (object) cursor,
            (object) length
          };
          object obj = method.Invoke((object) null, parameters);
          cursor = (int) parameters[1];
          return (ICustom_Parameter) obj;
        }
        catch
        {
          cursor = num3;
        }
      }
      return (ICustom_Parameter) paramCustom;
    }

    public static ICustom_Parameter DecodeXmlNodeToCustomParameter(XmlNode node)
    {
      string[] strArray = node.Name.Split(new char[1]{ ':' });
      string key = "PARAM_" + strArray[strArray.Length - 1];
      if (key == "PARAM_Custom")
        return (ICustom_Parameter) PARAM_Custom.FromXmlNode(node);
      if (CustomParamDecodeFactory.vendorExtensionNameTypeHash != null)
      {
        try
        {
          Type type = (Type) CustomParamDecodeFactory.vendorExtensionNameTypeHash[(object) key];
          if (type != (Type) null)
          {
            MethodInfo method = type.GetMethod("FromXmlNode");
            if (method == (MethodInfo) null)
              return (ICustom_Parameter) null;
            object[] parameters = new object[1]
            {
              (object) node
            };
            return (ICustom_Parameter) method.Invoke((object) null, parameters);
          }
        }
        catch
        {
        }
      }
      return (ICustom_Parameter) null;
    }
  }
}
