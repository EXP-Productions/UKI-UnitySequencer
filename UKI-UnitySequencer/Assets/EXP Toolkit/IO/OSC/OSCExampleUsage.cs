using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;


//Usage 
//Add the OSCSettings prefab to your scene, it will instantiate a OSCHandler instance at runtime.
//Previous settings will be recalled at runtime from Player Prefs.
//Once an OSCHandler is active you can add Listeners to listen and retrieve data for a specific OSC address
//Or call OSCHandler.Instance.SendOSCMessage from anywhere to send messages to the list of clients.

public class OSCExampleUsage : MonoBehaviour {

    private OSCListener _Listener;
    public string _Address;
	// Use this for initialization
	void Start () {
        _Listener = new OSCListener(_Address);
        
        //set to true if you're interested in all OSC messages under the address, false only for that address.
        _Listener.m_ListenForChildren = true;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (_Listener.Updated)
        {
            while (_Listener.DataAvailable)
            {
                //if you're just appending a single float value you can get like this
                //float value = m_Listener.GetDataAsFloat();

                //if its a different data type you need to get it as an object then cast
                //object value = m_Listener.GetData();
                //string strValue = (string)value;

                //if you're sending an array you'll need to get the list and cast/handle individually

                KeyValuePair<string, List<object>> kv = _Listener.m_Data.Dequeue();
                string stringValue = kv.Value[0].ToString();
                string address = kv.Key;
                print(address + ":" + stringValue);
            }
        }
        
	}

    void SendData(object value)
    {
        //Create the message
        OSCMessage message = new OSCMessage("/test");

        //append some kind of data to the message.
        //message.AppendFloat(1.0f);

        //if you need a different data type use the below, supported types (SINGLE, DOUBLE, INT32, INT64 LONG, BYTE[], STRING)
        //message.Append<string>("Yo");

        OSCHandler.Instance.SendOSCMessage(message);
    }

}
