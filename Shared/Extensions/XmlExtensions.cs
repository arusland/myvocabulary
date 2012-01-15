using System;
using System.Xml;
using Shared.Helpers;

namespace Shared.Extensions
{
    public static class XmlExtensions
    {
        #region Methods

        #region Public

        public static XmlElement AddNode(this XmlNode node, string name, string value = null)
        {
            var result = node.OwnerDocument.CreateElement(name);

            if (value.IsNotNull())
            {
                result.InnerText = value;
            }

            node.AppendChild(result);

            return result;
        }

        public static XmlNode AddAttribute(this XmlNode node, string name, string value)
        {
            var attr = node.OwnerDocument.CreateAttribute(name);
            attr.Value = value;

            var result = node.Attributes.Append(attr);

            return node;
        }

        public static string GetAttribute(this XmlNode node, string attrName)
        {
            var result = GetAttributeSafe(node, attrName);

            if (result.IsEmpty())
            {
                throw new InvalidOperationException(string.Format("Attribute not found '{0}'.", attrName));
            }

            return result;
        }

        public static string GetAttributeSafe(this XmlNode node, string attrName)
        {
            XmlAttribute attr = node.Attributes[attrName];

            if (attr.IsNotNull())
            {
                return attr.Value;
            }

            return string.Empty;
        }

        public static string GetNodeValue(this XmlNode node, string nodeName)
        {
            var nodeChild = node.SelectSingleNode(nodeName);

            if (nodeChild.IsNull())
            {
                throw new InvalidOperationException(string.Format("Node not found '{0}'.", nodeName));
            }

            return nodeChild.InnerText;
        }

        public static string GetNodeValue(this XmlNode node)
        {
            var nodeChild = node.SelectSingleNode("value");

            Checker.NotNull(nodeChild, string.Format("Node 'value' not found in node '{0}'.", node.GetAttribute("name")));

            string result = nodeChild.InnerText;

            Checker.NotNullOrEmpty(result, string.Format("Node '{0}' not contains value.", node.GetAttribute("name")));

            return result;
        }

        #endregion

        #endregion
    }
}
