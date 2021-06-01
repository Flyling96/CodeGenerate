using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SerializationCodeVersionAsset:BaseSingleSetting<SerializationCodeVersionAsset>
{
    [Serializable]
    public struct VersionStruct
    {
        public int m_Version;
        public string m_VariableNames;
        public string m_VariableTypes;
    }
    [Serializable]
    public class TypeInfo
    {
        public List<VersionStruct> m_SerializationVersions;
        public string m_ResetVarialbleNames;
    }

    [Serializable]
    public class SerializedDictionaryVersion : SerializedDictionary<string, TypeInfo> { }

    public SerializedDictionaryVersion m_SerializationCodeVersionDic = new SerializedDictionaryVersion();


}
