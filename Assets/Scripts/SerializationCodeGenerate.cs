using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;
using System.Text;
using System.IO;

public class SerializationCodeGenerate
{
    public const string SavePath = "/Scripts/AutoGenerate/Serialization/";

    const string VirtualStr = "virtual";
    const string OverrideStr = "override";
    const string IfMacro = "#if ";
    const string UnityEditorMacro = IfMacro + "UNITY_EDITOR\n";
    const string EndIfMacro = "#endif\n";

    const string HeadStr = "using System;\n"+
    "using System.Collections;\n" +
    "using System.Collections.Generic;\n"+
    "using System.IO;\n"+
    "using UnityEngine;\n";

    const string ClassTitle = "public partial class {0}";

    const string DeserializeFuntionTitle = "\tpublic {0} void Deserialize_{1}(BinaryReader reader)";
    const string DeserializeBaseClassStr = "\t\tbase.Deserialize(reader);\n";
    //const string DeserializeBaseTypeFieldStr = "\t\tSerializationBinaryHelper.ReadBinary(ref {0},typeof({0}),reader);\n";

    const string DeserializeEnumFieldStr = "\t\t{0} = ({1})reader.ReadInt32();\n";
    const string DeserializeIntFieldStr = "\t\t{0} = reader.ReadInt32();\n";
    const string DeserializeFloatFieldStr = "\t\t{0} = reader.ReadSingle();\n";
    const string DeserializeBoolFieldStr = "\t\t{0} = reader.ReadBoolean();\n";
    const string DeserializeStringFieldStr = "\t\t{0} = reader.ReadString();\n";
    const string DeserializeVector3FieldStr = "\t\t{0} = reader.ReadVector3();\n";
    const string DeserializeVector2FieldStr = "\t\t{0} = reader.ReadVector2();\n";
    const string DeserializeInterfaceFieldStr = "\t\t{0} = new {1}();\n\t\t{0}.Deserialize(reader);\n";

    const string DeserializeArrayFieldStr =
        "\t\tint count = reader.ReadInt32();\n" +
        "\t\t{0} = new {1}[count];\n" +
        "\t\tfor(int i = 0; i < count; i++)\n" +
        "\t\t{{\n \t{2} \t\t}}\n";
    const string DeserializeListFieldStr =
        "\t\tint count = reader.ReadInt32();\n" +
        "\t\t{0} = new List<{1}>();\n"+
        "\t\t{0}.Capacity = count;\n"+
        "\t\tfor(int i = 0; i < count; i++)\n" +
        "\t\t{{\n \t{2} \t\t\t{0}.Add({3});\n \t\t}}\n";

    const string SerializeFuntionTitle = "\tpublic {0} void Serialize_{1}(BinaryWriter writer)";
    const string SerializeBaseClassStr = "\t\tbase.Serialize(writer);\n";
    //const string SerializeBaseTypeFieldStr = "\t\tSerializationBinaryHelper.WriteBinary(ref {0},typeof({0}),resder);\n";
    const string SerializeBaseTypeFieldStr = "\t\twriter.Write({0});\n";
    const string SerializeVectorTypeFieldStr = "\t\twriter.WriteVector({0});\n";
    const string SerializeEnumFieldStr = "\t\twriter.Write((int){0});\n";
    const string SerializeInterfaceFieldStr = "\t\t{0}.Serialize(writer);\n";

    const string SerializeArrayFieldStr =
        "\t\twriter.Write({0}.Length);\n" +
        "\t\tfor(int i = 0; i < {0}.Length; i++)\n" +
        "\t\t{{\n \t{1} \t\t}}\n";
    
    const string SerializeListFieldStr =
        "\t\twriter.Write({0}.Count);\n" +
        "\t\tfor(int i = 0; i < {0}.Count; i++)\n" +
        "\t\t{{\n \t{1} \t\t}}\n";

    [MenuItem("SerializationCode/Generate")]
    public static void GenerateCode()
    {
        var s = SerializationCodeVersionAsset.Instance;

        Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ISerialization))))
                        .ToArray();

        StringBuilder codeStr = new StringBuilder();

        for(int i =0;i < types.Length;i++)
        {
            SaveSerializationCode(types[i]);
        }

        AssetDatabase.Refresh();
        Debug.Log(codeStr.ToString());
    }

    private static void SaveSerializationCode(Type classType)
    {
        var versionInfo = CheckVersion(classType);
        if(versionInfo.Item1)
        {
            string classCode = GenerateClassCode(classType, versionInfo.Item2);
            string path = string.Format("{0}{1}{2}{3}.cs", Application.dataPath, SavePath, classType.Name,"Serialization");
            int startIndex = 0;
            if (File.Exists(path))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open))
                {
                    startIndex = (int)fileStream.Length;    
                }
            }
            else
            {
                classCode = string.Format("{0}\n{1}", HeadStr, classCode);
            }

            using(FileStream fileStream = new FileStream(path,FileMode.OpenOrCreate))
            {
                byte[] writerBytes = Encoding.UTF8.GetBytes(classCode);
                fileStream.Write(writerBytes,startIndex,writerBytes.Length);
                fileStream.Close();
            }
        }

    }

    private static (bool,int) CheckVersion(Type classType)
    {
        SerializationCodeVersionAsset.VersionListStruct versionList;
        var dic = SerializationCodeVersionAsset.Instance.m_SerializationCodeVersionDic;
        var binaryFields = classType.GetFields().Where(x => x.GetCustomAttribute(typeof(BinarySerializedFieldAttribute)) != null);

        StringBuilder variableInfos = new StringBuilder();
        foreach (var binaryField in binaryFields)
        {
            variableInfos.Append(binaryField.Name);
            variableInfos.Append(',');
            variableInfos.Append(binaryField.FieldType.FullName);
            variableInfos.Append('|');
        }
        string variableStr = variableInfos.ToString();

        int versionIndex = 10000;
        bool needNew = false;
        if (dic.TryGetValue(classType.FullName, out versionList))
        {
            if(versionList.m_Versions == null || versionList.m_Versions.Count < 1 )
            {
                if(versionList.m_Versions == null)
                {
                    versionList.m_Versions = new List<SerializationCodeVersionAsset.VersionStruct>();
                }
                SerializationCodeVersionAsset.VersionStruct version;
                version.m_Version = versionIndex;
                version.m_Variables = variableStr;
                versionList.m_Versions.Add(version);
                needNew = true;
            }
            else
            {
                var version = versionList.m_Versions[versionList.m_Versions.Count - 1];
                if(!string.Equals(version.m_Variables,variableStr))
                {
                    SerializationCodeVersionAsset.VersionStruct newVersion;
                    versionIndex = version.m_Version + 1;
                    newVersion.m_Version = versionIndex;
                    newVersion.m_Variables = variableStr;
                    versionList.m_Versions.Add(newVersion);
                    needNew = true;
                }
            }
        }
        else
        {
            SerializationCodeVersionAsset.VersionListStruct versionListStruct;
            versionListStruct.m_Versions = new List<SerializationCodeVersionAsset.VersionStruct>();
            SerializationCodeVersionAsset.VersionStruct version;
            version.m_Version = versionIndex;
            version.m_Variables = variableStr;
            versionListStruct.m_Versions.Add(version);
            dic.Add(classType.FullName, versionListStruct);
            needNew = true;
        }

        return (needNew, versionIndex);

    }

    private static string GenerateClassCode(Type classType,int version)
    {
        StringBuilder codeStr = new StringBuilder();
        Type baseType = classType.BaseType;
        bool isBaseClass = baseType == null || !typeof(ISerialization).IsAssignableFrom(baseType);
        codeStr.Append(string.Format(ClassTitle, classType.Name));
        codeStr.Append("\n{\n");
        codeStr.Append(string.Format(DeserializeFuntionTitle,isBaseClass?VirtualStr:OverrideStr, version));
        codeStr.Append("\n\t{\n");
        if(!isBaseClass)
        {
            codeStr.Append(DeserializeBaseClassStr);
        }
        var binaryFields = classType.GetFields().Where(x => x.GetCustomAttribute(typeof(BinarySerializedFieldAttribute)) != null);
        foreach(var binaryField in binaryFields)
        {
            Type fieldType = binaryField.FieldType;
            if (fieldType.IsArray)
            {
                codeStr.Append(DeserializeArrayField(binaryField));
            }
            else if(typeof(IList).IsAssignableFrom(fieldType))
            {
                codeStr.Append(DeserializeListField(binaryField));
            }
            else
            {
                codeStr.Append(DeserializeSingleField(binaryField.Name, fieldType));
            }
        }
        codeStr.Append("\t}\n");

        codeStr.Append(UnityEditorMacro);

        codeStr.Append(string.Format(SerializeFuntionTitle, isBaseClass ? VirtualStr : OverrideStr, version));
        codeStr.Append("\n\t{\n");

        if (!isBaseClass)
        {
            codeStr.Append(SerializeBaseClassStr);
        }

        foreach (var binaryField in binaryFields)
        {
            Type fieldType = binaryField.FieldType;
            if (fieldType.IsArray)
            {
                codeStr.Append(SerializeArrayField(binaryField));
            }
            else if (typeof(IList).IsAssignableFrom(fieldType))
            {
                codeStr.Append(SerializeListField(binaryField));
            }
            else
            {
                codeStr.Append(SerializeSingleField(binaryField.Name, fieldType));
            }
        }
        codeStr.Append("\t}\n");
        codeStr.Append(EndIfMacro);

        codeStr.Append("}\n\n");
        return codeStr.ToString();

    }


    private static string DeserializeArrayField(FieldInfo arrayField)
    {
        var elementType = arrayField.FieldType.GetElementType();
        if (elementType == null)
        {
            return string.Empty;
        }
        string singleStr = DeserializeSingleField(string.Format("{0}[i]", arrayField.Name),elementType);
        string arrayStr = string.Format(DeserializeArrayFieldStr, arrayField.Name,elementType.Name, singleStr);
        return arrayStr;
    }

    public static string DeserializeListField(FieldInfo listField)
    {
        var elementType = listField.FieldType.GetGenericArguments()[0];
        if(elementType == null)
        {
            return string.Empty;
        }
        string tempVariableName = "t_Element";
        string singleStr = DeserializeSingleField(string.Format("var {0}", tempVariableName), elementType);
        string listStr = string.Format(DeserializeListFieldStr, listField.Name, elementType.Name, singleStr, tempVariableName);
        return listStr;

    }

    private static string DeserializeSingleField(string name,Type fieldType)
    {
        //bool isBaseType = fieldType.IsPrimitive || fieldType == typeof(string) || fieldType == typeof(Vector3) || fieldType == typeof(Vector2);
        if (fieldType == typeof(int))
        {
            return string.Format(DeserializeIntFieldStr, name);
        }
        else if(fieldType == typeof(float))
        {
            return string.Format(DeserializeFloatFieldStr, name);
        }
        else if (fieldType == typeof(bool))
        {
            return string.Format(DeserializeBoolFieldStr, name);
        }
        else if (fieldType == typeof(string))
        {
            return string.Format(DeserializeStringFieldStr, name);
        }
        else if (fieldType == typeof(Vector3))
        {
            return string.Format(DeserializeVector3FieldStr, name);
        }
        else if (fieldType == typeof(Vector2))
        {
            return string.Format(DeserializeVector2FieldStr, name);
        }
        else if (fieldType.IsEnum)
        {
            return string.Format(DeserializeEnumFieldStr, name, fieldType.Name);
        }
        else
        {
            return string.Format(DeserializeInterfaceFieldStr, name, fieldType.Name);
        }
    }

    private static string SerializeArrayField(FieldInfo arrayField)
    {
        var elementType = arrayField.FieldType.GetElementType();
        if (elementType == null)
        {
            return string.Empty;
        }
        string singleStr = DeserializeSingleField(string.Format("{0}[i]", arrayField.Name), elementType);
        string arrayStr = string.Format(SerializeArrayFieldStr, arrayField.Name, singleStr);
        return arrayStr;
    }

    public static string SerializeListField(FieldInfo listField)
    {
        var elementType = listField.FieldType.GetGenericArguments()[0];
        if (elementType == null)
        {
            return string.Empty;
        }
        string singleStr = SerializeSingleField(string.Format("{0}[i]", listField.Name), elementType);
        string listStr = string.Format(SerializeListFieldStr, listField.Name, singleStr);
        return listStr;

    }

    private static string SerializeSingleField(string name, Type fieldType)
    {

        if (fieldType.IsPrimitive || fieldType == typeof(string))
        {
            return string.Format(SerializeBaseTypeFieldStr, name);
        }
        else if(fieldType == typeof(Vector3) || fieldType == typeof(Vector2))
        {
            return string.Format(SerializeVectorTypeFieldStr, name);
        }
        else if (fieldType.IsEnum)
        {
            return string.Format(SerializeEnumFieldStr, name);
        }
        else
        {
            return string.Format(SerializeInterfaceFieldStr, name);
        }
    }

}
