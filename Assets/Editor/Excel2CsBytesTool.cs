using System.IO;
using Excel;
using System.Data;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Table;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// 1.xlsx生成xml和c#结构类 2.(等待新生成的c#编译完成后)根据程序集中的c#表格结构类,读取对应的xml数据,反序列化到实体类数组中,然后再序列化到bytes文件.
/// </summary>
public class Excel2CsBytesTool
{
    static string ExcelDataPath = Application.dataPath + "/../ExcelData";//源Excel文件夹,xlsx格式
    static string XmlDataPath = Application.dataPath + "/../ExcelData/tempXmlData";//生成的xml(临时)文件夹.(可以在生成全部bytes完成后,代码删除该临时文件夹).
    static string BytesDataPath = Application.dataPath + "/Resources/DataTable";//生成的bytes文件夹
    static string CsClassPath = Application.dataPath + "/Scripts/DataTable";//生成的c#脚本文件夹
    static string AllCsHead = "all";//序列化结构体的数组类.类名前缀

    static void Init()
    {
        if (!Directory.Exists(CsClassPath))
        {
            Directory.CreateDirectory(CsClassPath);
        }
        if (!Directory.Exists(XmlDataPath))
        {
            Directory.CreateDirectory(XmlDataPath);
        }
        if (!Directory.Exists(BytesDataPath))
        {
            Directory.CreateDirectory(BytesDataPath);
        }
    }

    [MenuItem("SDGSupporter/Excel/1.xlsx生成xml和c#结构类")]
    static void Excel2XmlCs()
    {
        Init();

        string[] excelPaths = Directory.GetFiles(ExcelDataPath, "*.xlsx");
        for (int e = 0; e < excelPaths.Length; e++)
        {
            //0.读Excel
            string className;//类型名
            string[] names;//字段名
            string[] types;//字段类型
            string[] descs;//字段描述
            List<string[]> datasList;//数据

            try
            {
                string excelPath = excelPaths[e];//excel路径  
                className = Path.GetFileNameWithoutExtension(excelPath).ToLower();
                FileStream fileStream = File.Open(excelPath, FileMode.Open, FileAccess.Read);
                IExcelDataReader excelDataReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);
                // 表格数据全部读取到result里
                DataSet result = excelDataReader.AsDataSet();
                // 获取表格列数
                int columns = result.Tables[0].Columns.Count;
                // 获取表格行数
                int rows = result.Tables[0].Rows.Count;
                // 根据行列依次读取表格中的每个数据
                names = new string[columns];
                types = new string[columns];
                descs = new string[columns];
                datasList = new List<string[]>();
                for (int r = 0; r < rows; r++)
                {
                    string[] curRowData = new string[columns];
                    for (int c = 0; c < columns; c++)
                    {
                        //解析：获取第一个表格中指定行指定列的数据
                        curRowData[c] = result.Tables[0].Rows[r][c].ToString();
                    }
                    //解析：第一行类变量名
                    if (r == 0)
                    {
                        names = curRowData;
                    }//解析：第二行类变量类型
                    else if (r == 1)
                    {
                        types = curRowData;
                    }//解析：第三行类变量描述
                    else if (r == 2)
                    {
                        descs = curRowData;
                    }//解析：第三行开始是数据
                    else
                    {
                        datasList.Add(curRowData);
                    }
                }
            }
            catch (System.Exception exc)
            {
                Debug.LogError("请关闭Excel:" + exc.Message);
                return;
            }

            //1. 写Xml
            WriteXml(className, names, types, datasList);

            //2. 写Cs
            WriteCs(className, names, types, descs);
        }

        AssetDatabase.Refresh();
    }

    static void WriteCs(string className, string[] names, string[] types, string[] descs)
    {
        try
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using System.Collections.Generic;");
            stringBuilder.AppendLine("using System.IO;");
            stringBuilder.AppendLine("using System.Runtime.Serialization.Formatters.Binary;");
            stringBuilder.AppendLine("using System.Xml.Serialization;");
            stringBuilder.Append("\n");
            stringBuilder.AppendLine("namespace Table");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("    [Serializable]");
            stringBuilder.AppendLine("    public class " + className);
            stringBuilder.AppendLine("    {");
            for (int i = 0; i < names.Length; i++)
            {
                stringBuilder.AppendLine("        /// <summary>");
                stringBuilder.AppendLine("        /// " + descs[i]);
                stringBuilder.AppendLine("        /// </summary>");
                stringBuilder.AppendLine("        [XmlAttribute(\"" + names[i] + "\")]");
                stringBuilder.AppendLine("        public " + types[i] + " " + names[i] + ";");
                stringBuilder.Append("\n");
            }
            stringBuilder.AppendLine("        public static List<" + className + "> LoadBytes()");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine("            string bytesPath = \"" + BytesDataPath + "/" + className + ".bytes\";");
            stringBuilder.AppendLine("            if (!File.Exists(bytesPath))");
            stringBuilder.AppendLine("                return null;");
            stringBuilder.AppendLine("            using (FileStream stream = new FileStream(bytesPath, FileMode.Open))");
            stringBuilder.AppendLine("            {");
            stringBuilder.AppendLine("                BinaryFormatter binaryFormatter = new BinaryFormatter();");
            stringBuilder.AppendLine("                all" + className + " table = binaryFormatter.Deserialize(stream) as all" + className + ";");
            stringBuilder.AppendLine("                return table." + className + "s;");
            stringBuilder.AppendLine("            }");
            stringBuilder.AppendLine("        }");
            stringBuilder.AppendLine("    }");
            stringBuilder.Append("\n");
            stringBuilder.AppendLine("    [Serializable]");
            stringBuilder.AppendLine("    public class " + AllCsHead + className);
            stringBuilder.AppendLine("    {");
            stringBuilder.AppendLine("        public List<" + className + "> " + className + "s;");
            stringBuilder.AppendLine("    }");
            stringBuilder.AppendLine("}");

            string csPath = CsClassPath + "/" + className + ".cs";
            if (File.Exists(csPath))
            {
                File.Delete(csPath);
            }
            using (StreamWriter sw = new StreamWriter(csPath))
            {
                sw.Write(stringBuilder);
                Debug.Log("生成:" + csPath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("写入CS失败:" + e.Message);
            throw;
        }
    }

    static void WriteXml(string className, string[] names, string[] types, List<string[]> datasList)
    {
        try
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            stringBuilder.AppendLine("<" + AllCsHead + className + ">");
            stringBuilder.AppendLine("<" + className + "s>");
            for (int d = 0; d < datasList.Count; d++)
            {
                stringBuilder.Append("\t<" + className + " ");
                //单行数据
                string[] datas = datasList[d];
                for (int c = 0; c < datas.Length; c++)
                {
                    string name = names[c];
                    string data = datas[c];
                    stringBuilder.Append(name + "=\"" + data + "\" ");
                }
                stringBuilder.Append("/>");
                stringBuilder.Append("\n");
            }
            stringBuilder.AppendLine("</" + className + "s>");
            stringBuilder.AppendLine("</" + AllCsHead + className + ">");

            string xmlPath = XmlDataPath + "/" + className + ".xml";
            if (File.Exists(xmlPath))
            {
                File.Delete(xmlPath);
            }
            using (StreamWriter sw = new StreamWriter(xmlPath))
            {
                sw.Write(stringBuilder);
                Debug.Log("生成文件:" + xmlPath);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("写入Xml失败:" + e.Message);
        }
    }

    [MenuItem("SDGSupporter/Excel/2.使用步骤1文件生成bytes(删除xml)")]
    static void Xml2Bytes()
    {
        Init();

        string csAssemblyPath = Application.dataPath + "/../Library/ScriptAssemblies/Assembly-CSharp.dll";
        Assembly assembly = Assembly.LoadFile(csAssemblyPath);
        if (assembly != null)
        {
            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (type.Namespace == "Table" && type.Name.Contains(AllCsHead))
                {
                    string className = type.Name.Replace(AllCsHead, "");

                    //读取xml数据
                    string xmlPath = XmlDataPath + "/" + className + ".xml";
                    if (!File.Exists(xmlPath))
                    {
                        Debug.LogError("Xml文件读取失败:" + xmlPath);
                        continue;
                    }
                    object table;
                    using (Stream reader = new FileStream(xmlPath, FileMode.Open))
                    {
                        //读取xml实例化table: all+classname
                        //object table = assembly.CreateInstance("Table." + type.Name);
                        XmlSerializer xmlSerializer = new XmlSerializer(type);
                        table = xmlSerializer.Deserialize(reader);
                    }
                    //obj序列化二进制
                    string bytesPath = BytesDataPath + "/" + className + ".bytes";
                    if (File.Exists(bytesPath))
                    {
                        File.Delete(bytesPath);
                    }
                    using (FileStream fileStream = new FileStream(bytesPath, FileMode.Create))
                    {
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        binaryFormatter.Serialize(fileStream, table);
                        Debug.Log("生成:" + bytesPath);
                    }
                    File.Delete(xmlPath);
                    Debug.Log("删除:" + bytesPath);
                }
            }
        }

        Directory.Delete(XmlDataPath);
        Debug.Log("删除:" + XmlDataPath);
    }
}
