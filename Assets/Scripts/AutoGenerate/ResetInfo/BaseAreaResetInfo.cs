using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class BaseArea: IResetInfo
{
	private List<object> m_InitInfos = new List<object>();
	public List<object> InitInfos
	{
		get{  return m_InitInfos; }
	}

	public virtual void RecordInfos()
	{
		m_InitInfos.Clear();
	}

	public virtual int ResetInfos()
	{
		int index = 0;
		return index;
	}

}
