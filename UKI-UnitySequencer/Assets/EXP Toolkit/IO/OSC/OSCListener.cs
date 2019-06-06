using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityOSC;
using Newtonsoft.Json;

[System.Serializable]
public class OSCListener
{
    public string m_Address = "/test";
    public bool m_ListenForChildren = false;
    public int m_DefualtValueIndex = 0;
    
    [JsonIgnore]
    public int m_CurrentMessageCount;
    [JsonIgnore]
    public Queue<KeyValuePair<string, List<object>>> m_Data;
    [JsonIgnore]
    public bool m_IsInitialized { get { if (m_Data == null) return false; else return true; } }
    [JsonIgnore]
    Dictionary<string, long> m_LastCheckTimeStamps = new Dictionary<string,long>();

    public OSCListener(string address)
    {
        m_Address = address;
        OSCHandler.Instance.AddListener(this, false);
        m_Data = new Queue<KeyValuePair<string, List<object>>>();
    }

    public void Init()
    {
        OSCHandler.Instance.AddListener(this, false);
    }

    public OSCListener(string address, bool doNotDestroyOnLoad)
    {
        m_Address = address;
        OSCHandler.Instance.AddListener(this, doNotDestroyOnLoad);
        m_Data = new Queue<KeyValuePair<string, List<object>>>();
    }

    public bool Updated
    {
        get
        {
            bool returnval = false;
            if (m_ListenForChildren)
            {
                IEnumerable<KeyValuePair<string, Queue<OSCPacket>>> dictEntries = OSCHandler.Instance.m_OSCAll.Where(i => i.Key.Contains(m_Address));

                foreach (KeyValuePair<string, Queue<OSCPacket>> kv in dictEntries)
                {
                    Queue<OSCPacket> oscPackets = kv.Value;
                    while (oscPackets.Count > 0)
                    {
                        OSCPacket oscPacket = oscPackets.Dequeue();
                        m_Data.Enqueue(new KeyValuePair<string, List<object>>(oscPacket.Address, oscPacket.Data));
                        returnval = true;
                    }
                }
            }
            else
            { 
                if (OSCHandler.Instance.m_OSCAll.ContainsKey(m_Address))
                {
                    while (OSCHandler.Instance.m_OSCAll[m_Address].Count > 0)
                    {
                        OSCPacket oscPack = OSCHandler.Instance.m_OSCAll[m_Address].Dequeue();
                        m_Data.Enqueue(new KeyValuePair<string, List<object>>(oscPack.Address, oscPack.Data));
                        returnval = true;
                    }
                }
            }
            return returnval;
        }
    }

    public bool DataAvailable
    {
        get
        {
            return m_Data.Count > 0;
        }
    }

    public void ClearQueue()
    {
        bool update = Updated;
        m_Data.Clear();
    }

    public List<object> GetAllData(out string address)
    {
        KeyValuePair<string, List<object>> kv = m_Data.Dequeue();
        address = kv.Key;
        return kv.Value;
    }

    public List<object> GetAllData()
    {
        return m_Data.Dequeue().Value;
    }


    public object GetData(int index)
    {
        return m_Data.Dequeue().Value[index];
    }

    public Byte[] GetBytes()
    {
        if (m_Data != null)
        {
            Byte[] bytes = (Byte[])m_Data.Dequeue().Value[0];
            return bytes;
        }
        else
        {
            Debug.Log("Data is null");
            return null;
        }

    }

    public float GetDataAsFloat(int index)
    {
        return (float)m_Data.Dequeue().Value[index];        
    }

    public float GetDataAsFloat()
    {
        return (float)m_Data.Dequeue().Value[m_DefualtValueIndex];
    }
}
