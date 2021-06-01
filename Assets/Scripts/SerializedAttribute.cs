using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[AttributeUsage(AttributeTargets.Field)]
public class BinarySerializedFieldAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class BinarySerializedClassAttribute: Attribute
{

}
