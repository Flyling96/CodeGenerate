using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BinarySerializedClass]
public partial class SpecialArea : BaseArea
{

    int[] AreaIDs = new int[10];

    [BinarySerializedField]
    int temp = 2;
}
