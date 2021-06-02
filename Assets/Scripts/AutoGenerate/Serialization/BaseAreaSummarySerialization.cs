using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class BaseArea: ISerialization
{
	public virtual void Deserialize(BinaryReader reader)
	{
		int version = reader.ReadInt32();
		switch(version)
		{
			case 10000:Deserialize_10000(reader);break;
			case 10001:Deserialize_10001(reader);break;
		}
	}

#if UNITY_EDITOR
	public virtual void Serialize(BinaryWriter writer)
	{
		int version = 10000;
		SerializationCodeVersionAsset.TypeInfo typeInfo;
		if(SerializationCodeVersionAsset.Instance.m_SerializationCodeVersionDic.TryGetValue(GetType().FullName,out typeInfo))
		{
			version = typeInfo.m_SerializationVersions[typeInfo.m_SerializationVersions.Count - 1].m_Version;
		}
		writer.Write(version);
		switch(version)
		{
			case 10000:Serialize_10000(writer);break;
			case 10001:Serialize_10001(writer);break;
		}
	}

#endif
}
