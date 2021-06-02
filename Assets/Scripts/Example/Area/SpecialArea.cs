using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BinarySerializedClass]
public partial class SpecialArea : BaseArea
{
    [CanReset]
    int[] AreaIDs = new int[10];

    [BinarySerialized]
    int temp = 2;

    Quaternion m_QuaternionTest;

    [CanReset]
    [BinarySerialized]
    public Quaternion QuaternionTest
    {
        get
        {
            return m_QuaternionTest;
        }
        set
        {
            m_QuaternionTest = value;
        }
    }
}
