using System.Xml;
using LLRPReaderUI_Avalonia.Models;
using Org.LLRP.LTK.LLRPV1.DataType;

namespace LLRPReaderUI_Avalonia.ViewModels
{
    /// <summary>
    /// LLRP消息扩展方法（通用版本）
    /// 通过解析 ToString() 输出的 XML 来构建树状结构
    /// </summary>
    public static class LLRPMessageExtensionsPro
    {
        /// <summary>
        /// 通过 ToString() 得到的 XML 来构建整个树状结构
        /// 虽然效率不高，但可以保证覆盖所有参数和子参数，且不需要针对每个消息类型进行单独处理
        /// </summary>
        public static LLRPMessageNode BuildTreeFromMSG(this Message msg)
        {
            if (msg == null)
                return new LLRPMessageNode("null");

            string xmlString = msg.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(xmlString))
                return new LLRPMessageNode(msg.GetType().Name);

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xmlString);

                // 获取根元素
                XmlNode? rootElement = doc.DocumentElement;
                if (rootElement == null)
                    return new LLRPMessageNode(msg.GetType().Name);

                var rootNode = new LLRPMessageNode(rootElement.Name);
                BuildTreeFromXmlNode(rootElement, rootNode);
                return rootNode;
            }
            catch (XmlException)
            {
                // XML 解析失败，返回原始字符串
                var node = new LLRPMessageNode(msg.GetType().Name);
                node.AddChild("ToString()", xmlString);
                return node;
            }
        }

        /// <summary>
        /// 从 XML 节点构建树状结构
        /// </summary>
        private static void BuildTreeFromXmlNode(XmlNode xmlNode, LLRPMessageNode treeNode)
        {
            // 处理属性
            if (xmlNode.Attributes != null && xmlNode.Attributes.Count > 0)
            {
                foreach (XmlAttribute attr in xmlNode.Attributes)
                {
                    treeNode.AddChild($"@{attr.Name}", attr.Value);
                }
            }

            // 处理子节点
            foreach (XmlNode child in xmlNode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Text)
                {
                    // 文本内容作为值
                    string? text = child.Value?.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        treeNode.AddChild("#text", text);
                    }
                }
                else if (child.NodeType == XmlNodeType.Element)
                {
                    // 检查是否是叶子节点（只有文本内容，没有子元素）
                    bool isLeafNode = true;
                    foreach (XmlNode grandChild in child.ChildNodes)
                    {
                        if (grandChild.NodeType == XmlNodeType.Element)
                        {
                            isLeafNode = false;
                            break;
                        }
                    }

                    if (isLeafNode)
                    {
                        // 叶子节点：直接添加值
                        string? textContent = child.InnerText?.Trim();
                        var childNode = treeNode.AddChild(child.Name, textContent);

                        // 处理属性
                        if (child.Attributes != null && child.Attributes.Count > 0)
                        {
                            foreach (XmlAttribute attr in child.Attributes)
                            {
                                childNode.AddChild($"@{attr.Name}", attr.Value);
                            }
                        }
                    }
                    else
                    {
                        // 非叶子节点：递归处理
                        var childNode = treeNode.AddChild(child.Name);

                        // 处理属性
                        if (child.Attributes != null && child.Attributes.Count > 0)
                        {
                            foreach (XmlAttribute attr in child.Attributes)
                            {
                                childNode.AddChild($"@{attr.Name}", attr.Value);
                            }
                        }

                        BuildTreeFromXmlNode(child, childNode);
                    }
                }
            }
        }
    }
}
