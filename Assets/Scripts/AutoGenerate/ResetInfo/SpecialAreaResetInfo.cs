using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class SpecialArea
{
	private List<object> m_InitInfos = new List<object>();
	public List<object> InitInfos
	{
		get{  return m_InitInfos; }
	}

	public override void RecordInfos()
	{
		base.RecordInfos();
		m_InitInfos.Clear();
		m_InitInfos.Add(AreaIDs);
		m_InitInfos.Add(QuaternionTest);
	}

	public override int ResetInfos()
	{
		int index = base.ResetInfos();
		AreaIDs = (Int32[])m_InitInfos[index++];
		QuaternionTest = (Quaternion)m_InitInfos[index++];
		return index;
	}

}
