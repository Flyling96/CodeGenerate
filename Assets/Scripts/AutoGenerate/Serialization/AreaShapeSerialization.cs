using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class AreaShape
{
	public virtual void Deserialize_10000(BinaryReader reader)
	{
		m_Type = (ShapeType)reader.ReadInt32();
		m_Center = reader.ReadVector3();
		int count = reader.ReadInt32();
		m_ShapeData = new List<Vector3>();
		m_ShapeData.Capacity = count;
		for(int i = 0; i < count; i++)
		{
 			var t_Element = reader.ReadVector3();
 			m_ShapeData.Add(t_Element);
 		}
	}
#if UNITY_EDITOR
	public virtual void Serialize_10000(BinaryWriter writer)
	{
		writer.Write((int)m_Type);
		writer.WriteVector(m_Center);
		writer.Write(m_ShapeData.Count);
		for(int i = 0; i < m_ShapeData.Count; i++)
		{
 			writer.WriteVector(m_ShapeData[i]);
 		}
	}
#endif
}

