using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CanResetAttribute : Attribute { }

public interface IResetInfo
{
    List<object> InitInfos { get; }

    void RecordInfos();

    void ResetInfos();
}