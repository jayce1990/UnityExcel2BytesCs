using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace Table
{
    [Serializable]
    public class weapon
    {
        /// <summary>
        /// 编号
        /// </summary>
        [XmlAttribute("id")]
        public int id;

        /// <summary>
        /// 名字
        /// </summary>
        [XmlAttribute("name")]
        public string name;

        /// <summary>
        /// 预制体名
        /// </summary>
        [XmlAttribute("prefabName")]
        public string prefabName;

        /// <summary>
        /// 描述
        /// </summary>
        [XmlAttribute("desc")]
        public string desc;

        /// <summary>
        /// 数量
        /// </summary>
        [XmlAttribute("nums")]
        public int nums;

        public static List<weapon> LoadBytes()
        {
            string bytesPath = "D:/Code/Excel2XmlBytes/Assets/Resources/DataTable/weapon.bytes";
            if (!File.Exists(bytesPath))
                return null;
            using (FileStream stream = new FileStream(bytesPath, FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                allweapon table = binaryFormatter.Deserialize(stream) as allweapon;
                return table.weapons;
            }
        }
    }

    [Serializable]
    public class allweapon
    {
        public List<weapon> weapons;
    }
}
