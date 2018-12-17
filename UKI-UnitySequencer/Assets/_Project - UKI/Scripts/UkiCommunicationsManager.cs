using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UkiCommunicationsManager : ThreadedUDPReceiver
{
    private static uint[] _HeartBeatMessage = new uint[] {240, 0, 0};


    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SendHeartBeat", 1f, 1f);   
    }
    
    void SendHeartBeat()
    {
        SendInts(_HeartBeatMessage);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
