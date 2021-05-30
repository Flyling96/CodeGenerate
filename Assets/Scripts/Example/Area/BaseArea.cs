using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class BaseArea : ISerialization
{
    [BinarySerializedField]
    public AreaShape m_Shape;
    protected List<long> m_InSideActorIdList = new List<long>();

    public bool ConvertRot()
    {
        return m_Shape.m_Type == AreaShape.ShapeType.Box;
    }

    public void Init(AreaShape shape)
    {
        m_InSideActorIdList.Clear();
        m_Shape = shape;
    }

    public virtual bool EnterTrigger(long uid, Vector3 pos)
    {
        if (m_InSideActorIdList.Contains(uid))
        {
            return false;
        }

        if (m_Shape.IsInside(pos))

        {
            m_InSideActorIdList.Add(uid);
            return true;
        }

        return false;
    }

    public virtual bool StayTrigger(long uid, Vector3 pos)
    {
        if (!m_InSideActorIdList.Contains(uid))
        {
            return false;
        }

        if (m_Shape.IsInside(pos))
        {
            return true;
        }

        return false;
    }

    public virtual bool ExitTrigger(long uid, Vector3 pos)
    {
        if (!m_InSideActorIdList.Contains(uid))
        {
            return false;
        }

        if (!m_Shape.IsInside(pos))
        {
            m_InSideActorIdList.Remove(uid);
            return true;
        }

        return false;
    }


    public virtual void Deserialize(BinaryReader reader)
    {
        m_Shape = new AreaShape();
        m_Shape.Deserialize(reader);
    }

}

#if UNITY_EDITOR
[Serializable]
public partial class BaseArea
{
    public virtual void Serialize(BinaryWriter writer)
    {
        m_Shape.Serialize(writer);
    }
}
#endif
