using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class SerializedDictionary<TKey,TValue>:Dictionary<TKey, TValue>,ISerializationCallbackReceiver
{
    [SerializeField]
    protected List<TKey> m_Keys = new List<TKey>();

    [SerializeField]
    protected List<TValue> m_Values = new List<TValue>();

    public void OnBeforeSerialize()
    {
        m_Keys.Clear();
        m_Values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            m_Keys.Add(pair.Key);
            m_Values.Add(pair.Value);
        }

    }

    //Load the dictionary from lists
    public void OnAfterDeserialize()
    {
        this.Clear();

        for (int i = 0; i < m_Keys.Count; i++)
        {
            this.Add(m_Keys[i], m_Values[i]);
        }
    }
}
