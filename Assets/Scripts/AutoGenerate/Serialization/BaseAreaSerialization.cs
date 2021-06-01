using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class BaseArea
{
	private void Deserialize_10000(BinaryReader reader)
	{
		m_Shape = new AreaShape();
		m_Shape.Deserialize(reader);
	}

#if UNITY_EDITOR
	private void Serialize_10000(BinaryWriter writer)
	{
		m_Shape.Serialize(writer);
	}
#endif
}

