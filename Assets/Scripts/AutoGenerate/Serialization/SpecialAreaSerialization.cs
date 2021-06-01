using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class SpecialArea
{
	private void Deserialize_10000(BinaryReader reader)
	{
		temp = reader.ReadInt32();
	}

#if UNITY_EDITOR
	private void Serialize_10000(BinaryWriter writer)
	{
		writer.Write(temp);
	}
#endif
}

