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

	public virtual void RecordInfos()
	{
		m_InitInfos.Clear();
		m_InitInfos.Add(m_Type);
		m_InitInfos.Add(m_Center);
	}

	public virtual int ResetInfos()
	{
		int index = 0;
		m_Type = (ShapeType)m_InitInfos[index++];
		m_Center = (Vector3)m_InitInfos[index++];
		return index;
	}

}
