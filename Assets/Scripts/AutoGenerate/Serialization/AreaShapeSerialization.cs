using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class AreaShape
{
	private void Deserialize_10000(BinaryReader reader)
	{
		m_Type = (ShapeType)reader.ReadInt32();
		m_Center = reader.ReadVector3();
	}

#if UNITY_EDITOR
	private void Serialize_10000(BinaryWriter writer)
	{
		writer.Write((int)m_Type);
		writer.WriteVector(m_Center);
	}
#endif
}

