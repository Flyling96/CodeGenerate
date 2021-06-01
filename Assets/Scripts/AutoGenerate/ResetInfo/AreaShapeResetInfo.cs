using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class AreaShape: IResetInfo
{
	private List<object> m_InitInfos = new List<object>();
	public List<object> InitInfos
	{
		get{  return m_InitInfos; }
	}

	public void RecordInfos()
	{
		m_InitInfos.Clear();
		m_InitInfos.Capacity = 2;
		m_InitInfos.Add(m_Type);
		m_InitInfos.Add(m_Center);
	}

	public void ResetInfos()
	{
		m_Type = (ShapeType)m_InitInfos[0];
		m_Center = (Vector3)m_InitInfos[1];
	}

}
