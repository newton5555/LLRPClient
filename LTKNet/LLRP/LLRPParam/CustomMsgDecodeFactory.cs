using System;
using System.Collections;
using System.Reflection;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
  public class CustomMsgDecodeFactory
  {
    public static Hashtable vendorExtensionIDTypeHash;
    public static Hashtable vendorExtensionNameTypeHash;
    public static Hashtable vendorExtensionAssemblyHash;

    public static void LoadVendorExtensionAssembly(Assembly asm)
    {
      if (CustomMsgDecodeFactory.vendorExtensionIDTypeHash == null)
        CustomMsgDecodeFactory.vendorExtensionIDTypeHash = new Hashtable();
      if (CustomMsgDecodeFactory.vendorExtensionNameTypeHash == null)
        CustomMsgDecodeFactory.vendorExtensionNameTypeHash = new Hashtable();
      if (CustomMsgDecodeFactory.vendorExtensionAssemblyHash == null)
        CustomMsgDecodeFactory.vendorExtensionAssemblyHash = new Hashtable();
      string name = asm.GetName().Name;
      if (CustomMsgDecodeFactory.vendorExtensionAssemblyHash.ContainsKey((object) name))
        return;
      CustomMsgDecodeFactory.vendorExtensionAssemblyHash.Add((object) name, (object) asm);
      try
      {
        foreach (Type type in asm.GetTypes())
        {
          if (!(type.BaseType != typeof (MSG_CUSTOM_MESSAGE)))
          {
            string typeName = type.Namespace + "." + type.Name;
            MSG_CUSTOM_MESSAGE instance = (MSG_CUSTOM_MESSAGE) asm.CreateInstance(typeName);
            string key = instance.VendorID.ToString() + "-" + instance.SubType.ToString();
            if (!CustomMsgDecodeFactory.vendorExtensionIDTypeHash.ContainsKey((object) key))
              CustomMsgDecodeFactory.vendorExtensionIDTypeHash.Add((object) key, (object) type);
            if (!CustomMsgDecodeFactory.vendorExtensionNameTypeHash.ContainsKey((object) type.Name))
              CustomMsgDecodeFactory.vendorExtensionNameTypeHash.Add((object) type.Name, (object) type);
          }
        }
      }
      catch
      {
        Console.WriteLine("LVEA failed", (object) asm);
      }
    }

    public static MSG_CUSTOM_MESSAGE DecodeCustomMessage(
      ref BitArray bit_array,
      ref int cursor,
      int length)
    {
      if (cursor >= length)
        return (MSG_CUSTOM_MESSAGE) null;
      int num1 = cursor;
      MSG_CUSTOM_MESSAGE msgCustomMessage = MSG_CUSTOM_MESSAGE.FromBitArray(ref bit_array, ref cursor, length);
      if (msgCustomMessage == null)
        return (MSG_CUSTOM_MESSAGE) null;
      string key = msgCustomMessage.VendorID.ToString() + "-" + msgCustomMessage.SubType.ToString();
      if (CustomMsgDecodeFactory.vendorExtensionIDTypeHash != null)
      {
        int num2 = cursor;
        try
        {
          MethodInfo method = ((Type) CustomMsgDecodeFactory.vendorExtensionIDTypeHash[(object) key]).GetMethod("FromBitArray");
          if ((MethodInfo) null == method)
            return (MSG_CUSTOM_MESSAGE) null;
          cursor = num1;
          object[] parameters = new object[3]
          {
            (object) bit_array,
            (object) cursor,
            (object) length
          };
          object obj = method.Invoke((object) null, parameters);
          cursor = (int) parameters[1];
          return (MSG_CUSTOM_MESSAGE) obj;
        }
        catch
        {
          cursor = num2;
        }
      }
      return msgCustomMessage;
    }

    public static MSG_CUSTOM_MESSAGE DecodeXmlNodeToCustomMessage(XmlNode node, string xmlstr)
    {
      if (CustomMsgDecodeFactory.vendorExtensionNameTypeHash != null)
      {
        string[] strArray = node.Name.Split(new char[1]
        {
          ':'
        });
        string key = "MSG_" + strArray[strArray.Length - 1];
        try
        {
          Type type = (Type) CustomMsgDecodeFactory.vendorExtensionNameTypeHash[(object) key];
          if ((Type) null != type)
          {
            MethodInfo method = type.GetMethod("FromString");
            if ((MethodInfo) null == method)
              return (MSG_CUSTOM_MESSAGE) null;
            object[] parameters = new object[1]
            {
              (object) xmlstr
            };
            return (MSG_CUSTOM_MESSAGE) method.Invoke((object) null, parameters);
          }
        }
        catch
        {
        }
      }
      return (MSG_CUSTOM_MESSAGE) null;
    }
  }
}
