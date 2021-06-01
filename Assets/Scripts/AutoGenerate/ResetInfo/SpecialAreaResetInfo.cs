using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class SpecialArea: IResetInfo
{
	private List<object> m_InitInfos = new List<object>();
	public List<object> InitInfos
	{
		get{  return m_InitInfos; }
	}

	public void RecordInfos()
	{
		m_InitInfos.Clear();
		m_InitInfos.Capacity = 1;
		m_InitInfos.Add(AreaIDs);
	}

	public void ResetInfos()
	{
		AreaIDs = (Int32[])m_InitInfos[0];
	}

}
