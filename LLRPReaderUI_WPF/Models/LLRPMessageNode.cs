using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Org.LLRP.LTK.LLRPV1;

namespace LLRPReaderUI_WPF.Models
{
    /// <summary>
    /// 表示LLRP消息树中的一个节点
    /// </summary>
    public class LLRPMessageNode
    {
    public LLRPMessageNode(string name, string? value = null, string? description = null)
    {
        Name = name;
        Value = value;
        Description = description;
        Children = new ObservableCollection<LLRPMessageNode>();
    }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 节点值
        /// </summary>
        public string? Value { get; }

        /// <summary>
        /// 节点描述
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// 子节点
        /// </summary>
        public ObservableCollection<LLRPMessageNode> Children { get; }

        /// <summary>
        /// 添加子节点
        /// </summary>
        public LLRPMessageNode AddChild(string name, string? value = null, string? description = null)
        {
            var child = new LLRPMessageNode(name, value, description);
            Children.Add(child);
            return child;
        }

        /// <summary>
        /// 递归构建树状显示的字符串
        /// </summary>
        public string BuildTreeString(string indent = "")
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{indent}{Name}");

            if (!string.IsNullOrEmpty(Value))
            {
                sb.AppendLine($"{indent}  Value: {Value}");
            }

            if (!string.IsNullOrEmpty(Description))
            {
                sb.AppendLine($"{indent}  Description: {Description}");
            }

            foreach (var child in Children)
            {
                sb.Append(child.BuildTreeString(indent + "  "));
            }

            return sb.ToString();
        }
    }
}
