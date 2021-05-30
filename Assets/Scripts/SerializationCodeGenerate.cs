using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;
using System.Text;

public class SerializationCodeGenerate
{
    public const string SavePath = "Asset/Scripts/GetCustomAttribute/";

    const string VirtualStr = "virtual";
    const string OverrideStr = "override";
    const string IfMacro = "#if ";
    const string UnityEditorMacro = IfMacro + "UNITY_EDITOR\n";
    const string EndIfMacro = "#endif\n";

    const string ClassTitle = "public partial class {0}";
    const string DeserializeFuntionTitle = "\tpublic {0} void Deserialize(BinaryReader reader)";
    const string DeserializeBaseTypeFieldStr = "\t\tSerializationBinaryHelper.ReadBinary(ref {0},typeof({0}),resder);\n";
    const string DeserializeEnumFieldStr = "\t\t{0} = ({1})Reader.ReadInt32();\n";
    const string DeserializeInterfaceFieldStr = "\t\t{0} = new {1}();\n\t\t{0}.Deserialize(reader);\n";

    const string SerializeFuntionTitle = "\tpublic {0} void Serialize(BinaryWriter writer)";
    const string SerializeBaseTypeFieldStr = "\t\tSerializationBinaryHelper.WriteBinary(ref {0},typeof({0}),resder);\n";
    const string SerializeEnumFieldStr = "\t\twriter.Write((int){0});\n";
    const string SerializeInterfaceFieldStr = "\t\t{0}.Serialize(writer);\n";


    [MenuItem("SerializationCode/Generate")]
    public static void GenerateCode()
    {
        Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ISerialization))))
                        .ToArray();

        StringBuilder codeStr = new StringBuilder();

        for(int i =0;i < types.Length;i++)
        {
            GenerateClassCode(types[i], codeStr);
        }

        Debug.Log(codeStr.ToString());
    }

    public static void GenerateClassCode(Type classType, StringBuilder codeStr)
    {
        Type baseType = classType.BaseType;
        bool isBaseClass = baseType == null || !typeof(ISerialization).IsAssignableFrom(baseType);
        codeStr.Append(string.Format(ClassTitle, classType.Name));
        codeStr.Append("\n{\n");
        codeStr.Append(string.Format(DeserializeFuntionTitle,isBaseClass?VirtualStr:OverrideStr));
        codeStr.Append("\n\t{\n");
        var binarayFields = classType.GetFields().Where(x => x.GetCustomAttribute(typeof(BinarySerializedFieldAttribute)) != null);
        foreach(var binaryField in binarayFields)
        {
            Type fieldType = binaryField.FieldType;
            bool isBaseType = fieldType.IsPrimitive || fieldType == typeof(string) || fieldType == typeof(Vector3) || fieldType == typeof(Vector2);
            if(isBaseType)
            {
                codeStr.Append(string.Format(DeserializeBaseTypeFieldStr, binaryField.Name));
            }
            else if(fieldType.IsEnum)
            {
                codeStr.Append(string.Format(DeserializeEnumFieldStr, binaryField.Name, fieldType.Name));
            }
            else
            {
                codeStr.Append(string.Format(DeserializeInterfaceFieldStr,binaryField.Name,fieldType.Name));
            }
        }
        codeStr.Append("\t}\n");

        codeStr.Append(UnityEditorMacro);

        codeStr.Append(string.Format(SerializeFuntionTitle, isBaseClass ? VirtualStr : OverrideStr));
        codeStr.Append("\n\t{\n");

        foreach (var binaryField in binarayFields)
        {
            Type fieldType = binaryField.FieldType;
            bool isBaseType = fieldType.IsPrimitive || fieldType == typeof(string) || fieldType == typeof(Vector3) || fieldType == typeof(Vector2);
            if (isBaseType)
            {
                codeStr.Append(string.Format(SerializeBaseTypeFieldStr, binaryField.Name));
            }
            else if (fieldType.IsEnum)
            {
                codeStr.Append(string.Format(SerializeEnumFieldStr, binaryField.Name));
            }
            else
            {
                codeStr.Append(string.Format(SerializeInterfaceFieldStr, binaryField.Name));
            }
        }
        codeStr.Append("\t}\n");
        codeStr.Append(EndIfMacro);

        codeStr.Append("}\n\n");

    }

}
