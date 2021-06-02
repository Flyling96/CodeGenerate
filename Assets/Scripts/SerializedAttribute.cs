using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class BinarySerializedAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class BinarySerializedClassAttribute: Attribute
{

}
