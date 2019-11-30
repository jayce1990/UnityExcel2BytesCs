using System;
using System.Collections.Generic;
using System.Xml.Serialization;

/// <summary>
/// Excel2CsBytesTool
/// Excel可以配置的数组类型：string[] int[] bool[] 
/// 可自行扩展
/// </summary>
namespace Table
{
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class stringArray
    {
        [System.Xml.Serialization.XmlElementAttribute("item")]
        public List<string> item { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class intArray
    {
        [System.Xml.Serialization.XmlElementAttribute("item")]
        public List<int> item { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public class boolArray
    {
        [System.Xml.Serialization.XmlElementAttribute("item")]
        public List<bool> item { get; set; }
    }
}
