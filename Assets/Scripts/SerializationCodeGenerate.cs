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

    const string ClassTitle = "public partial class {0}\n";
    const string SerializationClassTitle = "public partial class {0}: ISerialization\n";

    const string DeserializeFunctionTitle = "\tprivate void Deserialize_{0}(BinaryReader reader)";
    const string DeserializeSummaryFunctionTitle = "\tpublic {0} void Deserialize(BinaryReader reader)";
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
    const string DeleteDeserializeInterfaceFieldStr = "\t\tvar t_{0} = new {1}();\n\t\t{0}.Deserialize(reader);\n";

    const string DeserializeArrayFieldStr =
        "\t\tint count = reader.ReadInt32();\n" +
        "\t\t{0} = new {1}[count];\n" +
        "\t\tfor(int i = 0; i < count; i++)\n" +
        "\t\t{{\n \t{2} \t\t}}\n";
    const string DeleteDeserializeArrayFieldStr =
        "\t\tint count = reader.ReadInt32();\n" +
        "\t\tfor(int i = 0; i < count; i++)\n" +
        "\t\t{{\n \t{0} \t\t}}\n";
    const string DeserializeListFieldStr =
        "\t\tint count = reader.ReadInt32();\n" +
        "\t\t{0} = new List<{1}>();\n"+
        "\t\t{0}.Capacity = count;\n"+
        "\t\tfor(int i = 0; i < count; i++)\n" +
        "\t\t{{\n \t{2} \t\t\t{0}.Add({3});\n \t\t}}\n";

    const string DeserializeVersionStr = "\t\tint version = reader.ReadInt32();\n";
    const string SwitchVersionStr = "\t\tswitch(version)\n\t\t{\n";
    const string DeserializeCaseFunctionStr = "\t\t\tcase {0}:Deserialize_{0}(reader);break;\n";


    const string SerializeFunctionTitle = "\tprivate void Serialize_{0}(BinaryWriter writer)";
    const string SerializeSummaryFunctionTitle = "\tpublic {0} void Serialize(BinaryWriter writer)";
    const string SerializeBaseClassStr = "\t\tbase.Serialize(writer);\n";
    //const string SerializeBaseTypeFieldStr = "\t\tSerializationBinaryHelper.WriteBinary(ref {0},typeof({0}),resder);\n";
    const string SerializeBaseTypeFieldStr = "\t\twriter.Write({0});\n";
    const string SerializeVectorTypeFieldStr = "\t\twriter.WriteVector({0});\n";
    const string SerializeEnumFieldStr = "\t\twriter.Write((int){0});\n";
    const string SerializeInterfaceFieldStr = "\t\t{0}.Serialize(writer);\n";
    const string DeleteSerializeInterfaceFieldStr = "\t\tvar t_{0} = new {1}();\n\t\t{0}.Serialize(writer);\n";

    const string SerializeArrayFieldStr =
        "\t\twriter.Write({0}.Length);\n" +
        "\t\tfor(int i = 0; i < {0}.Length; i++)\n" +
        "\t\t{{\n \t{1} \t\t}}\n";
    
    const string SerializeListFieldStr =
        "\t\twriter.Write({0}.Count);\n" +
        "\t\tfor(int i = 0; i < {0}.Count; i++)\n" +
        "\t\t{{\n \t{1} \t\t}}\n";

    const string SerializeVersionStr = "\t\tint version = 10000;\n"+
        "\t\tSerializationCodeVersionAsset.VersionListStruct versionList;\n" +
        "\t\tif (SerializationCodeVersionAsset.Instance.m_SerializationCodeVersionDic.TryGetValue(GetType().FullName,out versionList))\n\t\t{\n" +
        "\t\t\tversion = versionList.m_Versions[versionList.m_Versions.Count - 1].m_Version;\n\t\t}\n" +
        "\t\twriter.Write(version);\n";

    const string SerializeCaseFunctionStr = "\t\t\tcase {0}:Serialize_{0}(writer);break;\n";


    [MenuItem("SerializationCode/Generate")]
    public static void GenerateCode()
    {
        Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes().Where(t => t.GetCustomAttribute(typeof(BinarySerializedClassAttribute))!=null))
                        .ToArray();

        for(int i =0;i < types.Length;i++)
        {
            SaveSerializationCode(types[i]);
        }

        AssetDatabase.Refresh();
    }

    private static void SaveSerializationCode(Type classType)
    {
        var versionInfo = CheckVersion(classType);
        if(versionInfo.Item1)
        {
            DeleteFieldCheck(classType);
            string classCode = GenerateSerializationFunctionCode(classType, versionInfo.Item2);
            string path = string.Format("{0}{1}{2}{3}.cs", Application.dataPath, SavePath, classType.Name,"Serialization");
            if (File.Exists(path))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open))
                {
                    classCode = string.Format("\n{0}}}\n\n", classCode);
                    byte[] writerBytes = Encoding.UTF8.GetBytes(classCode);
                    fileStream.Seek(-3, SeekOrigin.End);
                    fileStream.Write(writerBytes, 0, writerBytes.Length);
                }
            }
            else
            {
                string classTitle = string.Format(ClassTitle, classType.Name);
                classCode = string.Format("{0}\n{1}{{\n{2}}}\n\n", HeadStr, classTitle, classCode);
                using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    byte[] writerBytes = Encoding.UTF8.GetBytes(classCode);
                    fileStream.Seek(0, SeekOrigin.Begin);
                    fileStream.Write(writerBytes, 0, writerBytes.Length);
                }
            }
            SaveSummarySerializationCode(classType);
        }

    }

    private static void DeleteFieldCheck(Type classType)
    {
        string path = string.Format("{0}{1}{2}{3}.cs", Application.dataPath, SavePath, classType.Name, "Serialization");

        var binaryFieldsAttribute = classType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
          .Where(x => x.GetCustomAttribute(typeof(BinarySerializedFieldAttribute))!= null).
          Select(x => (Name:x.Name, Attribute:x.GetCustomAttribute(typeof(BinarySerializedFieldAttribute)) as BinarySerializedFieldAttribute));

        var dic = SerializationCodeVersionAsset.Instance.m_SerializationCodeVersionDic;
        SerializationCodeVersionAsset.VersionListStruct versionList;
        if (dic.TryGetValue(classType.FullName, out versionList))
        {
            if (versionList.m_Versions == null || versionList.m_Versions.Count < 2)
            {
                return;
            }

            string[] preFieldNames = versionList.m_Versions[versionList.m_Versions.Count - 2].m_VariableNames.Split(';');
            string[] preFieldTypeStrings = versionList.m_Versions[versionList.m_Versions.Count - 2].m_VariableTypes.Split(';');
            Type[] preFieldTypes = new Type[preFieldTypeStrings.Length];
            for(int i =0; i< preFieldTypeStrings.Length;i++)
            {
                var typeString = preFieldTypeStrings[i];
                preFieldTypes[i] = Type.GetType(typeString);

            }
            string[] nowFieldNames = versionList.m_Versions[versionList.m_Versions.Count - 1].m_VariableNames.Split(';');

            using (FileStream fileStream = new FileStream(path, FileMode.Open))
            {
                var readByte = new byte[fileStream.Length];
                fileStream.Read(readByte, 0, (int)fileStream.Length);
                var codeStr = Encoding.UTF8.GetString(readByte);

                List<string> deleteFields = new List<string>();
                for (int i = 0; i < preFieldNames.Length; i++)
                {
                    var preFieldName = preFieldNames[i];
                    if (!nowFieldNames.Contains(preFieldName))
                    {
                        var preFieldType = preFieldTypes[i];
                        if (preFieldType.GetCustomAttribute(typeof(BinarySerializedFieldAttribute)) != null)
                        {
                            string originStr = string.Format(DeserializeInterfaceFieldStr, preFieldName, preFieldType);
                            string replaceStr = string.Format(DeleteDeserializeInterfaceFieldStr, preFieldName, preFieldType);
                            codeStr = codeStr.Replace(originStr, replaceStr);
                            originStr = string.Format(SerializeInterfaceFieldStr, preFieldName);
                            replaceStr = string.Format(DeleteSerializeInterfaceFieldStr, preFieldName, preFieldType);
                            codeStr = codeStr.Replace(originStr, replaceStr);
                        }
                        else if (preFieldType.IsArray || typeof(IList).IsAssignableFrom(preFieldType))
                        {
                            var elementType = preFieldType.IsArray ? preFieldType.GetElementType() : preFieldType.GetGenericArguments()[0];
                            string originStr = "";
                            if (preFieldType.IsArray)
                            {
                                originStr = DeserializeArrayField(preFieldName, elementType);
                            }
                            else
                            {
                                originStr = DeserializeListField(preFieldName, elementType);
                            }
                            string singleStr = DeserializeSingleField(string.Format("var t_{0}", preFieldName), elementType);
                            string replaceStr = string.Format(DeleteDeserializeArrayFieldStr, singleStr);
                            codeStr = codeStr.Replace(originStr, replaceStr);
                            originStr = SerializeArrayField(preFieldName, elementType);
                            replaceStr = "\t\twriter.Write(0);\n";
                            codeStr = codeStr.Replace(originStr, replaceStr);

                        }
                        else
                        {
                            string defaultStr = GetTypeDefault(preFieldType);
                            codeStr = codeStr.Replace(string.Format("{0} = ", preFieldName), string.Empty);
                            codeStr = codeStr.Replace(preFieldName, defaultStr);
                        }
                    }
                }
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.SetLength(0);
                byte[] writeBytes = Encoding.UTF8.GetBytes(codeStr);
                fileStream.Write(writeBytes, 0, writeBytes.Length);
            }
        }
    }

    private static string GetTypeDefault(Type type)
    {
        if(type == typeof(int) || type == typeof(float))
        {
            return "0";
        }
        else if(type == typeof(bool))
        {
            return "false";
        }
        else if(type == typeof(Vector2))
        {
            return "Vector2.zero";
        }
        else if(type == typeof(Vector3))
        {
            return "Vector3.zero";
        }
        else
        {
            return string.Empty;
        }
    }

    private static void SaveSummarySerializationCode(Type classType)
    {     
        string classCode = GenerateSummarySerializationFunctionCode(classType);
        string path = string.Format("{0}{1}{2}{3}.cs", Application.dataPath, SavePath, classType.Name, "SummarySerialization");
        using (FileStream fileStream = new FileStream(path, FileMode.Create))
        {
            byte[] writerBytes = Encoding.UTF8.GetBytes(classCode);
            fileStream.Write(writerBytes, 0, writerBytes.Length);
        }
    }

    private static (bool,int) CheckVersion(Type classType)
    {
        SerializationCodeVersionAsset.VersionListStruct versionList;
        var dic = SerializationCodeVersionAsset.Instance.m_SerializationCodeVersionDic;
        var binaryFields = classType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.GetCustomAttribute(typeof(BinarySerializedFieldAttribute)) != null);

        StringBuilder variableNames = new StringBuilder();
        StringBuilder variableTypes = new StringBuilder();
        foreach (var binaryField in binaryFields)
        {
            variableNames.Append(binaryField.Name);
            variableNames.Append(';');
            variableTypes.Append(binaryField.FieldType.FullName);
            variableTypes.Append(';');
        }
        string variableNameStr = variableNames.ToString();
        variableNameStr = variableNameStr.Remove(variableNameStr.Length - 1);
        string variableTypeStr = variableTypes.ToString();
        variableTypeStr = variableTypeStr.Remove(variableTypeStr.Length - 1);

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
                version.m_VariableNames = variableNameStr;
                version.m_VariableTypes = variableTypeStr;
                versionList.m_Versions.Add(version);
                needNew = true;
            }
            else
            {
                var version = versionList.m_Versions[versionList.m_Versions.Count - 1];
                if(!string.Equals(version.m_VariableNames,variableNameStr))
                {
                    SerializationCodeVersionAsset.VersionStruct newVersion;
                    versionIndex = version.m_Version + 1;
                    newVersion.m_Version = versionIndex;
                    newVersion.m_VariableNames = variableNameStr;
                    newVersion.m_VariableTypes = variableTypeStr;
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
            version.m_VariableNames = variableNameStr;
            version.m_VariableTypes = variableTypeStr;
            versionListStruct.m_Versions.Add(version);
            dic.Add(classType.FullName, versionListStruct);
            needNew = true;
        }

        return (needNew, versionIndex);

    }

    private static string GenerateSerializationFunctionCode(Type classType,int version)
    {
        StringBuilder codeStr = new StringBuilder();
        Type baseType = classType.BaseType;

        codeStr.Append(string.Format(DeserializeFunctionTitle, version));
        codeStr.Append("\n\t{\n");

        var binaryFields = classType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetCustomAttribute(typeof(BinarySerializedFieldAttribute)) != null);
        foreach(var binaryField in binaryFields)
        {
            Type fieldType = binaryField.FieldType;
            if (fieldType.IsArray)
            {
                codeStr.Append(DeserializeArrayField(binaryField.Name, binaryField.FieldType.GetElementType())) ;
            }
            else if(typeof(IList).IsAssignableFrom(fieldType))
            {
                codeStr.Append(DeserializeListField(binaryField.Name, binaryField.FieldType.GetGenericArguments()[0]));
            }
            else
            {
                codeStr.Append(DeserializeSingleField(binaryField.Name, fieldType));
            }
        }
        codeStr.Append("\t}\n\n");

        codeStr.Append(UnityEditorMacro);

        codeStr.Append(string.Format(SerializeFunctionTitle,  version));
        codeStr.Append("\n\t{\n");

        foreach (var binaryField in binaryFields)
        {
            Type fieldType = binaryField.FieldType;
            if (fieldType.IsArray)
            {
                codeStr.Append(SerializeArrayField(binaryField.Name, binaryField.FieldType.GetElementType()));
            }
            else if (typeof(IList).IsAssignableFrom(fieldType))
            {
                codeStr.Append(SerializeListField(binaryField.Name, binaryField.FieldType.GetGenericArguments()[0]));
            }
            else
            {
                codeStr.Append(SerializeSingleField(binaryField.Name, fieldType));
            }
        }
        codeStr.Append("\t}\n");
        codeStr.Append(EndIfMacro);
        return codeStr.ToString();

    }

    private static string GenerateSummarySerializationFunctionCode(Type classType)
    {
        StringBuilder codeStr = new StringBuilder();
        Type baseType = classType.BaseType;
        bool isBaseClass = baseType == null || baseType.GetCustomAttribute(typeof(BinarySerializedClassAttribute)) == null;
        codeStr.Append(HeadStr);
        codeStr.Append("\n");

        codeStr.Append(string.Format(SerializationClassTitle, classType.Name));
        codeStr.Append("{\n");

        codeStr.Append(string.Format(DeserializeSummaryFunctionTitle, isBaseClass ? VirtualStr : OverrideStr));
        codeStr.Append("\n\t{\n");
        if (!isBaseClass)
        {
            codeStr.Append(DeserializeBaseClassStr);
        }
        codeStr.Append(DeserializeVersionStr);
        codeStr.Append(SwitchVersionStr);

        SerializationCodeVersionAsset.VersionListStruct versionList;
        if (SerializationCodeVersionAsset.Instance.m_SerializationCodeVersionDic.TryGetValue(classType.FullName, out versionList))
        {
            for(int i =0; i< versionList.m_Versions.Count;i++)
            {
                codeStr.Append(string.Format(DeserializeCaseFunctionStr, versionList.m_Versions[i].m_Version));
            }
        }

        codeStr.Append("\t\t}\n");
        codeStr.Append("\t}\n\n");
        codeStr.Append(UnityEditorMacro);

        codeStr.Append(string.Format(SerializeSummaryFunctionTitle, isBaseClass ? VirtualStr : OverrideStr));
        codeStr.Append("\n\t{\n");
        if (!isBaseClass)
        {
            codeStr.Append(SerializeBaseClassStr);
        }
        codeStr.Append(SerializeVersionStr);
        codeStr.Append(SwitchVersionStr);
        if (versionList.m_Versions != null)
        {
            for (int i = 0; i < versionList.m_Versions.Count; i++)
            {
                codeStr.Append(string.Format(SerializeCaseFunctionStr, versionList.m_Versions[i].m_Version));
            }
        }
        codeStr.Append("\t\t}\n");
        codeStr.Append("\t}\n\n");
        codeStr.Append(EndIfMacro);
        codeStr.Append("}\n");

        return codeStr.ToString();
    }

    private static string DeserializeArrayField(string fieldName,Type elementType)
    {
        if (elementType == null)
        {
            return string.Empty;
        }
        string singleStr = DeserializeSingleField(string.Format("{0}[i]", fieldName),elementType);
        string arrayStr = string.Format(DeserializeArrayFieldStr, fieldName, elementType.Name, singleStr);
        return arrayStr;
    }

    public static string DeserializeListField(string fieldName, Type elementType)
    {
        //var elementType = listField.FieldType.GetGenericArguments()[0];
        if(elementType == null)
        {
            return string.Empty;
        }
        string tempVariableName = "t_Element";
        string singleStr = DeserializeSingleField(string.Format("var {0}", tempVariableName), elementType);
        string listStr = string.Format(DeserializeListFieldStr, fieldName, elementType.Name, singleStr, tempVariableName);
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

    private static string SerializeArrayField(string fieldName, Type elementType)
    {
        if (elementType == null)
        {
            return string.Empty;
        }
        string singleStr = SerializeSingleField(string.Format("{0}[i]", fieldName), elementType);
        string arrayStr = string.Format(SerializeArrayFieldStr, fieldName, singleStr);
        return arrayStr;
    }

    public static string SerializeListField(string fieldName, Type elementType)
    {
        if (elementType == null)
        {
            return string.Empty;
        }
        string singleStr = SerializeSingleField(string.Format("{0}[i]", fieldName), elementType);
        string listStr = string.Format(SerializeListFieldStr, fieldName, singleStr);
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
