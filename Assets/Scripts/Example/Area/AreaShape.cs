using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



public partial class AreaShape : ISerialization
{
    public enum ShapeType
    {
        Sphere,
        Box,
        Prism,
    }
    [BinarySerializedField]
    public ShapeType m_Type = ShapeType.Sphere;
    [BinarySerializedField]
    public Vector3 m_Center;
    [BinarySerializedField]
    public List<Vector3> m_ShapeData = new List<Vector3>();
    //Sphere,0  radius
    //Box,0 size
    //Prism,vertex Pos

    public virtual bool IsInside(Vector3 point)
    {
        if (m_ShapeData.Count == 0)
        {
            return false;
        }

        if (m_Type == ShapeType.Box)
        {
            Vector3 size = m_ShapeData[0];
            Vector3 offset = point - m_Center;
            if (Mathf.Abs(offset.x) > size.x / 2 || Mathf.Abs(offset.y) > size.y / 2 || Mathf.Abs(offset.z) > size.z / 2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else if (m_Type == ShapeType.Sphere)
        {
            Vector3 size = m_ShapeData[0];
            Vector3 offset = point - m_Center;
            return offset.magnitude < size.x;
        }
        else if (m_Type == ShapeType.Prism)
        {
            int count = 0;
            Vector3 vv, vp, vvcvp;
            for (int i = 0; i < m_ShapeData.Count; i++)
            {
                int pre = i == 0 ? m_ShapeData.Count - 1 : i - 1;
                vv = new Vector3(m_ShapeData[i].x - m_ShapeData[pre].x, 0, m_ShapeData[i].z - m_ShapeData[pre].z);
                vp = new Vector3(point.x - (m_ShapeData[pre].x + m_Center.x), 0, point.z - (m_ShapeData[pre].z + m_Center.z));
                vvcvp = Vector3.Cross(vv, vp);
                if (vvcvp.y <= 0)
                {
                    count++;
                }
            }

            if (count == m_ShapeData.Count || count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }


    public void Deserialize(BinaryReader reader)
    {
        m_Type = (ShapeType)reader.ReadInt32();
        m_Center = reader.ReadVector3();
        int count = reader.ReadInt32();
        m_ShapeData.Clear();
        for (int i = 0; i < count; i++)
        {
            m_ShapeData.Add(reader.ReadVector3());
        }
    }
}

#if UNITY_EDITOR
[Serializable]
public partial class AreaShape
{
    public void Serialize(BinaryWriter writer)
    {
        writer.Write((int)m_Type);
        writer.WriteVector(m_Center);
        writer.Write(m_ShapeData.Count);
        for (int i = 0; i < m_ShapeData.Count; i++)
        {
            writer.WriteVector(m_ShapeData[i]);
        }
    }
}
#endif
