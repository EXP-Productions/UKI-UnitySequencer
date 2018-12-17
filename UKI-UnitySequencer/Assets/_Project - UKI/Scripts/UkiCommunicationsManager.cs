﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UkiCommunicationsManager : ThreadedUDPReceiver
{
    private static uint[] _HeartBeatMessage = new uint[] {240, 0, 0};

    public static UkiCommunicationsManager Instance {  get { return _Instance; } }
    private static UkiCommunicationsManager _Instance;

    public override void Awake()
    {
        base.Awake();
        _Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SendHeartBeat", 1f, 1f);   
    }
    

    public void SendActuatorMessage(Actuator actuator)
    {
        uint[] actuatorMessage = new uint[3];
        actuatorMessage[0] = (uint)actuator._ActuatorIndex;
        actuatorMessage[1] = (uint)ModBusRegisters.MB_GOTO_POSITION;
        actuatorMessage[2] = (uint)actuator._CurrentLinearLength;
        SendInts(actuatorMessage);
    }

    void SendHeartBeat()
    {
        SendInts(_HeartBeatMessage);
    }
}
