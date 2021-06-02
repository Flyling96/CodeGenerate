using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ResetInfoCodeGenerate
{
    public const string SavePath = "/Scripts/AutoGenerate/ResetInfo/";

    const string VirtualStr = "virtual";
    const string OverrideStr = "override";

    const string HeadStr = "using System;\n" +
    "using System.Collections.Generic;\n" +
    "using System.IO;\n" +
    "using UnityEngine;\n";

    const string BaseClassTitle = "public partial class {0}: IResetInfo\n{{\n{1}}}\n";
    const string ClassTitle = "public partial class {0}\n{{\n{1}}}\n";

    const string PrivateInitInfosField = "\tprivate List<object> m_InitInfos = new List<object>();\n";
    const string InitInfosProperty = "\tpublic List<object> InitInfos\n\t{\n\t\t" +
        "get{  return m_InitInfos; }\n\t}\n\n";

    const string RecordInfoFunctionTitle = "\tpublic {0} void RecordInfos()\n";
    const string RecordInfoBaseStr = "\t\tbase.RecordInfos();\n";
    const string RecordInfoInitInfosStr = "\t\tm_InitInfos.Clear();\n";
    const string RecordInfoSingleStr = "\t\tm_InitInfos.Add({0});\n";
    const string ResetInfoFunctionTitle = "\tpublic {0} int ResetInfos()\n";
    const string ResetInfoInitIndexStr = "\t\tint index = 0;\n";
    const string ResetInfoBaseStr = "\t\tint index = base.ResetInfos();\n";
    const string ResetInfoSingleStr = "\t\t{0} = ({1})m_InitInfos[index++];\n";
    const string ResetInfoReturnStr = "\t\treturn index;\n";

    [MenuItem("GenerateCode/ResetInfo")]
    public static void GenerateCode()
    {
        Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes().Where(t => t.GetCustomAttribute(typeof(BinarySerializedClassAttribute)) != null))
                        .ToArray();

        for(int i =0; i< types.Length;i++)
        {
            SaveResetInfoCode(types[i]);
        }

        AssetDatabase.Refresh();
    }

    private static void SaveResetInfoCode(Type classType)
    {
        if(CheckVersion(classType))
        {
            string dicPath = string.Format("{0}{1}", Application.dataPath, SavePath);
            if (!Directory.Exists(dicPath))
            {
                Directory.CreateDirectory(dicPath);
            }

            Type baseType = classType.BaseType;
            bool isBaseClass = baseType == null || baseType.GetCustomAttribute(typeof(BinarySerializedClassAttribute)) == null;
            string classCode = string.Format("{0}\n{1}", HeadStr, isBaseClass?BaseClassTitle:ClassTitle);
            string functionCode = GenerateResetInfoFunctionCode(classType);
            classCode = string.Format(classCode, classType.Name,functionCode);
            string path = string.Format("{0}{1}{2}{3}.cs", Application.dataPath, SavePath, classType.Name, "ResetInfo");

            using (FileStream fileStream = new FileStream(path,FileMode.OpenOrCreate))
            {
                fileStream.SetLength(0);
                byte[] writerBytes = Encoding.UTF8.GetBytes(classCode);
                fileStream.Write(writerBytes, 0, writerBytes.Length);
            }
        }
    }

    private static bool CheckVersion(Type classType)
    {
        bool isBaseClass = classType.BaseType == null || classType.BaseType.GetCustomAttribute(typeof(BinarySerializedClassAttribute)) == null;
        SerializationCodeVersionAsset.TypeInfo typeInfo;
        var dic = SerializationCodeVersionAsset.Instance.m_SerializationCodeVersionDic;
        var resetInfoFields = classType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.GetCustomAttribute(typeof(CanResetAttribute)) != null);

        var resetInfoProperties = classType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.GetCustomAttribute(typeof(CanResetAttribute)) != null);

        if (resetInfoFields.Count() < 1 && resetInfoProperties.Count() < 1 && !isBaseClass)
        {
            return false;
        }

        StringBuilder resetVariableNames = new StringBuilder();
        foreach (var resetInfoField in resetInfoFields)
        {
            resetVariableNames.Append(resetInfoField.Name);
            resetVariableNames.Append(';');
        }

        foreach (var resetInfoProperty in resetInfoProperties)
        {
            resetVariableNames.Append(resetInfoProperty.Name);
            resetVariableNames.Append(';');
        }

        string resetVariableNamesStr = resetVariableNames.ToString();
        if (resetVariableNamesStr.Length > 0)
        {
            resetVariableNamesStr = resetVariableNamesStr.Remove(resetVariableNamesStr.Length - 1);
        }

        if (dic.TryGetValue(classType.FullName, out typeInfo))
        {
            if(!string.Equals(typeInfo.m_ResetVarialbleNames, resetVariableNamesStr))
            {
                typeInfo.m_ResetVarialbleNames = resetVariableNamesStr;
                EditorUtility.SetDirty(SerializationCodeVersionAsset.Instance);
                AssetDatabase.SaveAssets();
                return true;
            }
            else
            {
                if (isBaseClass)
                {
                    string path = string.Format("{0}{1}{2}{3}.cs", Application.dataPath, SavePath, classType.Name, "ResetInfo");
                    if(!File.Exists(path))
                    {
                        return true;
                    }
                }
            }
        }
        else
        {
            typeInfo = new SerializationCodeVersionAsset.TypeInfo();
            typeInfo.m_SerializationVersions = new List<SerializationCodeVersionAsset.VersionStruct>();
            typeInfo.m_ResetVarialbleNames = resetVariableNamesStr;
            dic.Add(classType.FullName, typeInfo);
            EditorUtility.SetDirty(SerializationCodeVersionAsset.Instance);
            AssetDatabase.SaveAssets();
            return true;
        }

        return false;

    }

    private static string GenerateResetInfoFunctionCode(Type classType)
    {
        StringBuilder codeStr = new StringBuilder();
        Type baseType = classType.BaseType;
        bool isBaseClass = baseType == null || baseType.GetCustomAttribute(typeof(BinarySerializedClassAttribute)) == null;

        var resetInfoFields = classType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
          .Where(x => x.GetCustomAttribute(typeof(CanResetAttribute)) != null);
        var resetInfoProperties = classType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
          .Where(x => x.GetCustomAttribute(typeof(CanResetAttribute)) != null);

        codeStr.Append(PrivateInitInfosField);
        codeStr.Append(InitInfosProperty);

        codeStr.Append(string.Format(RecordInfoFunctionTitle, isBaseClass ? VirtualStr : OverrideStr));
        codeStr.Append("\t{\n");

        if(!isBaseClass)
        {
            codeStr.Append(RecordInfoBaseStr);
        }
        codeStr.Append(string.Format(RecordInfoInitInfosStr, resetInfoFields.Count()));
        foreach(var resetInfoField in resetInfoFields)
        {
            codeStr.Append(string.Format(RecordInfoSingleStr, resetInfoField.Name));
        }

        foreach(var resetInfoProperty in resetInfoProperties)
        {
            codeStr.Append(string.Format(RecordInfoSingleStr, resetInfoProperty.Name));
        }

        codeStr.Append("\t}\n\n");

        codeStr.Append(string.Format(ResetInfoFunctionTitle,isBaseClass?VirtualStr:OverrideStr));
        codeStr.Append("\t{\n");

        if (isBaseClass)
        {
            codeStr.Append(ResetInfoInitIndexStr);
        }
        else
        {
            codeStr.Append(ResetInfoBaseStr);
        }

        foreach (var resetInfoField in resetInfoFields)
        {
            codeStr.Append(string.Format(ResetInfoSingleStr, resetInfoField.Name, resetInfoField.FieldType.Name));
        }

        foreach (var resetInfoProperty in resetInfoProperties)
        {
            codeStr.Append(string.Format(ResetInfoSingleStr, resetInfoProperty.Name, resetInfoProperty.PropertyType.Name));
        }

        codeStr.Append(ResetInfoReturnStr);
        codeStr.Append("\t}\n\n");
        return codeStr.ToString();
    }
}
