using System;
using UnityEngine;
using System.IO;

public interface ISerialization
{
#if UNITY_EDITOR
        void Serialize(BinaryWriter writer);
#endif

    void Deserialize(BinaryReader reader);
}

public static class SerializationBinaryHelper
{
    public static void ReadBinary(ref object param, Type type, BinaryReader reader)
    {
        if (type == typeof(int))
        {
            param = reader.ReadInt32();
        }
        else if (type == typeof(float))
        {
            param = reader.ReadSingle();
        }
        else if (type == typeof(Vector3))
        {
            param = reader.ReadVector3();
        }
        else if (type == typeof(Vector2))
        {
            param = reader.ReadVector2();
        }
        else if (type == typeof(bool))
        {
            param = reader.ReadBoolean();
        }
        else if (type == typeof(string))
        {
            param = reader.ReadString();
        }
    }

    public static void WriteBinary(object param, Type type, BinaryWriter writer)
    {
        if (type == typeof(int))
        {
            writer.Write((int)param);
        }
        else if (type == typeof(float))
        {
            writer.Write((float)param);
        }
        else if (type == typeof(Vector3))
        {
            writer.WriteVector((Vector3)param);
        }
        else if (type == typeof(Vector2))
        {
            writer.WriteVector((Vector2)param);
        }
        else if (type == typeof(bool))
        {
            writer.Write((bool)param);
        }
        else if (type == typeof(string))
        {
            writer.Write((string)param);
        }
    }

    public static Vector3 ReadVector3(this BinaryReader reader)
    {
        Vector3 res = new Vector3();
        res.x = reader.ReadSingle();
        res.y = reader.ReadSingle();
        res.z = reader.ReadSingle();
        return res;
    }

    public static Vector3 ReadVector2(this BinaryReader reader)
    {
        Vector2 res = new Vector3();
        res.x = reader.ReadSingle();
        res.y = reader.ReadSingle();
        return res;
    }

    public static void WriteVector(this BinaryWriter writer,Vector3 vector)
    {
        writer.Write(vector.x);
        writer.Write(vector.y);
        writer.Write(vector.z);
    }

    public static void WriteVector(this BinaryWriter writer,Vector2 vector)
    {
        writer.Write(vector.x);
        writer.Write(vector.y);
    }

}

