using System;
using System.Collections;
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
		}
	}

#if UNITY_EDITOR
	public virtual void Serialize(BinaryWriter writer)
	{
		int version = 10000;
		SerializationCodeVersionAsset.TypeInfo versionList;
		if (SerializationCodeVersionAsset.Instance.m_SerializationCodeVersionDic.TryGetValue(GetType().FullName,out versionList))
		{
			version = versionList.m_SerializationVersions[versionList.m_SerializationVersions.Count - 1].m_Version;
		}
		writer.Write(version);
		switch(version)
		{
			case 10000:Serialize_10000(writer);break;
		}
	}

#endif
}
