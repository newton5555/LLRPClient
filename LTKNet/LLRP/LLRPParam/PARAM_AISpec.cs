

using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1
{
    ///<summary>
    ///
    ///This parameter defines antenna inventory operations.
    ///
    ///For more information, please refer to:
    ///
    ///<see cref="http://www.epcglobalinc.org/standards/llrp/llrp_1_0_1-standard-20070813.pdf#page=57&amp;view=fit">LLRP Specification Section 10.2.2,</see>
    ///<see cref="http://www.epcglobalinc.org/standards/llrp/llrp_1_0_1-standard-20070813.pdf#page=137&amp;view=fit">LLRP Specification Section 16.2.4.2,</see>
    ///</summary>
    public class PARAM_AISpec : Parameter
    {
        public UInt16Array AntennaIDs = new UInt16Array();
        private short AntennaIDs_len;
        public PARAM_AISpecStopTrigger AISpecStopTrigger;
        public PARAM_InventoryParameterSpec[] InventoryParameterSpec;
        public readonly CustomParameterArrayList Custom = new CustomParameterArrayList();

        public PARAM_AISpec() => this.typeID = (ushort)183;

        public bool AddCustomParameter(ICustom_Parameter param)
        {
            if (param is IAISpec_Custom_Param)
            {
                this.Custom.Add(param);
                return true;
            }
            if (!(param.GetType() == typeof(PARAM_Custom)))
                return false;
            this.Custom.Add(param);
            return true;
        }

        public static PARAM_AISpec FromBitArray(ref BitArray bit_array, ref int cursor, int length)
        {
            if (cursor >= length)
                return (PARAM_AISpec)null;
            int num1 = cursor;
            int num2 = length;
            ArrayList arrayList1 = new ArrayList();
            PARAM_AISpec paramAiSpec = new PARAM_AISpec();
            paramAiSpec.tvCoding = bit_array[cursor];
            int val;
            if (paramAiSpec.tvCoding)
            {
                ++cursor;
                val = (int)(ulong)Util.CalculateVal(ref bit_array, ref cursor, 7);
            }
            else
            {
                cursor += 6;
                val = (int)(ulong)Util.CalculateVal(ref bit_array, ref cursor, 10);
                paramAiSpec.length = (ushort)Util.DetermineFieldLength(ref bit_array, ref cursor);
                num2 = num1 + (int)paramAiSpec.length * 8;
            }
            if (val != (int)paramAiSpec.TypeID)
            {
                cursor = num1;
                return (PARAM_AISpec)null;
            }
            if (cursor > length || cursor > num2)
                throw new Exception("Input data is not a complete LLRP message");
            int fieldLength = Util.DetermineFieldLength(ref bit_array, ref cursor);
            object obj;
            Util.ConvertBitArrayToObj(ref bit_array, ref cursor, out obj, typeof(UInt16Array), fieldLength);
            paramAiSpec.AntennaIDs = (UInt16Array)obj;
            paramAiSpec.AISpecStopTrigger = PARAM_AISpecStopTrigger.FromBitArray(ref bit_array, ref cursor, length);
            ArrayList arrayList2 = new ArrayList();
            PARAM_InventoryParameterSpec inventoryParameterSpec;
            while ((inventoryParameterSpec = PARAM_InventoryParameterSpec.FromBitArray(ref bit_array, ref cursor, length)) != null)
                arrayList2.Add((object)inventoryParameterSpec);
            if (arrayList2.Count > 0)
            {
                paramAiSpec.InventoryParameterSpec = new PARAM_InventoryParameterSpec[arrayList2.Count];
                for (int index = 0; index < arrayList2.Count; ++index)
                    paramAiSpec.InventoryParameterSpec[index] = (PARAM_InventoryParameterSpec)arrayList2[index];
            }
            while (cursor < num2)
            {
                int num3 = cursor;
                ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeCustomParameter(ref bit_array, ref cursor, length);
                if (customParameter != null && cursor <= num2 && !paramAiSpec.AddCustomParameter(customParameter))
                {
                    cursor = num3;
                    break;
                }
            }
            return paramAiSpec;
        }

        public override string ToString()
        {
            string str = "<AISpec>" + "\r\n";
            if (this.AntennaIDs != null)
            {
                try
                {
                    str = str + "  <AntennaIDs>" + Util.ConvertArrayTypeToString((object)this.AntennaIDs, "u16v", "") + "</AntennaIDs>";
                    str += "\r\n";
                }
                catch
                {
                }
            }
            if (this.AISpecStopTrigger != null)
                str += Util.Indent(this.AISpecStopTrigger.ToString());
            if (this.InventoryParameterSpec != null)
            {
                int length = this.InventoryParameterSpec.Length;
                for (int index = 0; index < length; ++index)
                    str += Util.Indent(this.InventoryParameterSpec[index].ToString());
            }
            if (this.Custom != null)
            {
                int length = this.Custom.Length;
                for (int index = 0; index < length; ++index)
                    str += Util.Indent(this.Custom[index].ToString());
            }
            return str + "</AISpec>" + "\r\n";
        }

        public static PARAM_AISpec FromXmlNode(XmlNode node)
        {
            ArrayList arrayList = new ArrayList();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
            nsmgr.AddNamespace("", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
            nsmgr.AddNamespace("llrp", "http://www.llrp.org/ltk/schema/core/encoding/xml/1.0");
            PARAM_AISpec paramAiSpec = new PARAM_AISpec();
            string nodeValue = XmlUtil.GetNodeValue(node, "AntennaIDs");
            paramAiSpec.AntennaIDs = (UInt16Array)Util.ParseArrayTypeFromString(nodeValue, "u16v", "");
            try
            {
                XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "AISpecStopTrigger", nsmgr);
                if (xmlNodes != null)
                {
                    if (xmlNodes.Count != 0)
                        paramAiSpec.AISpecStopTrigger = PARAM_AISpecStopTrigger.FromXmlNode(xmlNodes[0]);
                }
            }
            catch
            {
            }
            try
            {
                XmlNodeList xmlNodes = XmlUtil.GetXmlNodes(node, "InventoryParameterSpec", nsmgr);
                if (xmlNodes != null)
                {
                    if (xmlNodes.Count != 0)
                    {
                        paramAiSpec.InventoryParameterSpec = new PARAM_InventoryParameterSpec[xmlNodes.Count];
                        for (int i = 0; i < xmlNodes.Count; ++i)
                            paramAiSpec.InventoryParameterSpec[i] = PARAM_InventoryParameterSpec.FromXmlNode(xmlNodes[i]);
                    }
                }
            }
            catch
            {
            }
            try
            {
                ArrayList nodeCustomChildren = XmlUtil.GetXmlNodeCustomChildren(node, nsmgr);
                if (nodeCustomChildren != null)
                {
                    for (int index = 0; index < nodeCustomChildren.Count; ++index)
                    {
                        if (!arrayList.Contains(nodeCustomChildren[index]))
                        {
                            ICustom_Parameter customParameter = CustomParamDecodeFactory.DecodeXmlNodeToCustomParameter((XmlNode)nodeCustomChildren[index]);
                            if (customParameter != null && paramAiSpec.AddCustomParameter(customParameter))
                                arrayList.Add(nodeCustomChildren[index]);
                        }
                    }
                }
            }
            catch
            {
            }
            return paramAiSpec;
        }

        public override void ToBitArray(ref bool[] bit_array, ref int cursor)
        {
            int num = cursor;
            if (this.tvCoding)
            {
                bit_array[cursor] = true;
                ++cursor;
                Util.ConvertIntToBitArray((uint)this.typeID, 7).CopyTo((Array)bit_array, cursor);
                cursor += 7;
            }
            else
            {
                cursor += 6;
                Util.ConvertIntToBitArray((uint)this.typeID, 10).CopyTo((Array)bit_array, cursor);
                cursor += 10;
                cursor += 16;
            }
            if (this.AntennaIDs != null)
            {
                try
                {
                    Util.ConvertIntToBitArray((uint)this.AntennaIDs.Count, 16).CopyTo((Array)bit_array, cursor);
                    cursor += 16;
                    BitArray bitArray = Util.ConvertObjToBitArray((object)this.AntennaIDs, (int)this.AntennaIDs_len);
                    bitArray.CopyTo((Array)bit_array, cursor);
                    cursor += bitArray.Length;
                }
                catch
                {
                }
            }
            if (this.AISpecStopTrigger != null)
                this.AISpecStopTrigger.ToBitArray(ref bit_array, ref cursor);
            if (this.InventoryParameterSpec != null)
            {
                int length = this.InventoryParameterSpec.Length;
                for (int index = 0; index < length; ++index)
                    this.InventoryParameterSpec[index].ToBitArray(ref bit_array, ref cursor);
            }
            if (this.Custom != null)
            {
                int length = this.Custom.Length;
                for (int index = 0; index < length; ++index)
                    this.Custom[index].ToBitArray(ref bit_array, ref cursor);
            }
            if (this.tvCoding)
                return;
            Util.ConvertIntToBitArray((uint)(cursor - num) / 8U, 16).CopyTo((Array)bit_array, num + 16);
        }

        public override void AppendToBitArray(AutoGrowingBitArray bArr)
        {
            int length1 = bArr.Length;
            if (this.tvCoding)
            {
                int length2 = bArr.Length;
                ++bArr.Length;
                bArr[length2] = true;
                Util.AppendIntToBitArray((uint)this.typeID, 7, bArr);
            }
            else
            {
                bArr.Length += 6;
                Util.AppendIntToBitArray((uint)this.typeID, 10, bArr);
                bArr.Length += 16;
            }
            if (this.AntennaIDs != null)
            {
                try
                {
                    Util.AppendIntToBitArray((uint)this.AntennaIDs.Count, 16, bArr);
                    Util.AppendObjToBitArray((object)this.AntennaIDs, (int)this.AntennaIDs_len, bArr);
                }
                catch
                {
                }
            }
            if (this.AISpecStopTrigger != null)
                this.AISpecStopTrigger.AppendToBitArray(bArr);
            if (this.InventoryParameterSpec != null)
            {
                int length3 = this.InventoryParameterSpec.Length;
                for (int index = 0; index < length3; ++index)
                    this.InventoryParameterSpec[index].AppendToBitArray(bArr);
            }
            if (this.Custom != null)
            {
                int length4 = this.Custom.Length;
                for (int index = 0; index < length4; ++index)
                    this.Custom[index].AppendToBitArray(bArr);
            }
            if (this.tvCoding)
                return;
            BitArray bitArray = Util.ConvertIntToBitArray((uint)(bArr.Length - length1) / 8U, 16);
            for (int index = 0; index < bitArray.Length; ++index)
                bArr[length1 + 16 + index] = bitArray[index];
        }
    }
}
