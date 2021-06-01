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
    public struct VersionListStruct
    {
        public List<VersionStruct> m_Versions;
    }

    [Serializable]
    public class SerializedDictionaryVersion : SerializedDictionary<string, VersionListStruct> { }

    public SerializedDictionaryVersion m_SerializationCodeVersionDic = new SerializedDictionaryVersion();


}
