
using System;
using System.Collections;
using System.Xml;


namespace Org.LLRP.LTK.LLRPV1.DataType
{
  public class XmlUtil
  {
    public static string GetNodeAttribute(
      XmlNode node,
      string child_node_name,
      string attribute_name)
    {
      foreach (XmlNode childNode in node.ChildNodes)
      {
        if (childNode.Name == child_node_name || childNode.LocalName == child_node_name)
        {
          XmlNode namedItem = childNode.Attributes.GetNamedItem(attribute_name);
          if (namedItem != null)
            return namedItem.InnerText;
          break;
        }
      }
      return string.Empty;
    }

    public static string GetNodeValue(XmlNode node, string child_node_name)
    {
      foreach (XmlNode childNode in node.ChildNodes)
      {
        if (childNode.Name == child_node_name || childNode.LocalName == child_node_name)
          return childNode.InnerText;
      }
      return string.Empty;
    }

    [Obsolete("Buggy, use GetXmlNodes that passes XmlNamespaceManager")]
    public static XmlNodeList GetXmlNodes(XmlNode node, string child_node_name)
    {
      if (node.NamespaceURI == null)
        return node.SelectNodes(child_node_name);
      XmlNamespaceManager nsmgr = new XmlNamespaceManager(node.OwnerDocument.NameTable);
      nsmgr.AddNamespace("llrp", node.NamespaceURI);
      return node.SelectNodes("llrp:" + child_node_name, nsmgr);
    }

    public static XmlNodeList GetXmlNodes(
      XmlNode node,
      string child_node_name,
      XmlNamespaceManager nsmgr)
    {
      IEnumerator enumerator = nsmgr.GetEnumerator();
      XmlNodeList xmlNodes;
      string xpath;
      for (xmlNodes = node.SelectNodes(child_node_name, nsmgr); xmlNodes.Count == 0 && enumerator.MoveNext(); xmlNodes = node.SelectNodes(xpath, nsmgr))
      {
        string current = (string) enumerator.Current;
        xpath = !("" != current) ? child_node_name : current + ":" + child_node_name;
      }
      return xmlNodes;
    }

    public static XmlNodeList GetXmlNodeChildren(XmlNode node, string child_node_name)
    {
      return node.SelectSingleNode(child_node_name)?.ChildNodes;
    }

    [Obsolete("Buggy, use GetXmlNodeCustomChildren that passes XmlNamespaceManager")]
    public static ArrayList GetXmlNodeCustomChildren(XmlNode node)
    {
      ArrayList nodeCustomChildren = new ArrayList();
      foreach (XmlNode childNode in node.ChildNodes)
      {
        if (childNode.Name.Contains(":"))
          nodeCustomChildren.Add((object) childNode);
      }
      return nodeCustomChildren;
    }

    public static ArrayList GetXmlNodeCustomChildren(XmlNode node, XmlNamespaceManager nsmgr)
    {
      ArrayList nodeCustomChildren = new ArrayList();
      foreach (XmlNode selectNode in node.SelectNodes("llrp:Custom", nsmgr))
        nodeCustomChildren.Add((object) selectNode);
      foreach (XmlNode childNode in node.ChildNodes)
      {
        if (childNode.Name.Contains(":") && !nodeCustomChildren.Contains((object) childNode))
          nodeCustomChildren.Add((object) childNode);
      }
      return nodeCustomChildren;
    }

    public static ArrayList GetXmlNodeCustomChildren(
      XmlNode node,
      string[] excl,
      XmlNamespaceManager nsmgr)
    {
      ArrayList nodeCustomChildren = new ArrayList();
      foreach (XmlNode selectNode in node.SelectNodes("llrp:Custom", nsmgr))
        nodeCustomChildren.Add((object) selectNode);
      foreach (XmlNode childNode in node.ChildNodes)
      {
        string[] strArray = childNode.Name.Split(new char[1]
        {
          ':'
        });
        if (1 < strArray.Length)
        {
          string str1 = strArray[strArray.Length - 1];
          bool flag = false;
          foreach (string str2 in excl)
          {
            if (str1 == str2)
            {
              flag = true;
              break;
            }
          }
          if (!flag)
            nodeCustomChildren.Add((object) childNode);
        }
      }
      return nodeCustomChildren;
    }

    public static string GetNodeAttrValue(XmlNode node, string attr_name)
    {
      foreach (XmlAttribute attribute in (XmlNamedNodeMap) node.Attributes)
      {
        if (attribute.Name == attr_name)
          return attribute.Value;
      }
      return string.Empty;
    }
  }
}
