using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class SpecialArea: ISerialization
{
	public override void Deserialize(BinaryReader reader)
	{
		base.Deserialize(reader);
		int version = reader.ReadInt32();
		switch(version)
		{
			case 10000:Deserialize_10000(reader);break;
			case 10001:Deserialize_10001(reader);break;
		}
	}

#if UNITY_EDITOR
	public override void Serialize(BinaryWriter writer)
	{
		base.Serialize(writer);
		int version = 10000;
		SerializationCodeVersionAsset.VersionListStruct versionList;
		if (SerializationCodeVersionAsset.Instance.m_SerializationCodeVersionDic.TryGetValue(GetType().FullName,out versionList))
		{
			version = versionList.m_Versions[versionList.m_Versions.Count - 1].m_Version;
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