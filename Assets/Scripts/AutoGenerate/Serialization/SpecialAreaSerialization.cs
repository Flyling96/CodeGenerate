using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class SpecialArea
{
	private void Deserialize_10000(BinaryReader reader)
	{
		int count = reader.ReadInt32();
		for(int i = 0; i < count; i++)
		{
 			var t_AreaIDs = reader.ReadInt32();
 		}
		temp = reader.ReadInt32();
	}

#if UNITY_EDITOR
	private void Serialize_10000(BinaryWriter writer)
	{
		writer.Write(0);
		writer.Write(temp);
	}
#endif

	private void Deserialize_10001(BinaryReader reader)
	{
		temp = reader.ReadInt32();
	}

#if UNITY_EDITOR
	private void Serialize_10001(BinaryWriter writer)
	{
		writer.Write(temp);
	}
#endif
}
