using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class UkiCommunicationsManager : ThreadedUDPReceiver
{
    public static int _CalibrateWaitTime = 60;

    private static Int16[] _HeartBeatMessage = new Int16[] {240, 0, 0};

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

    private void Update()
    {
        while(_ReceivedPackets.Count > 0)
        {
            byte[] packet = _ReceivedPackets.Dequeue();

            int actuatorIndex = System.BitConverter.ToUInt16(packet, 0);

            for (int i = 2; i < packet.Length; i+=4)
            {
                int registerIndex = System.BitConverter.ToUInt16(packet, i);
                int registerValue = System.BitConverter.ToUInt16(packet,  i+2);
                UkiStateDB._StateDB[actuatorIndex][registerIndex] = registerValue;
            }
        }        
    }

    public void SendPositionMessage(Actuator actuator)
    {
        Int16[] actuatorMessage = new Int16[3];
        actuatorMessage[0] = (Int16)actuator._ActuatorIndex;
        actuatorMessage[1] = (Int16)ModBusRegisters.MB_GOTO_POSITION;
        actuatorMessage[2] = (Int16)actuator._CurrentLinearLength;
        SendInts(actuatorMessage, true);
    }

    public void SendActuatorMessage(int index, Int16 length, ModBusRegisters register)
    {
        Int16[] actuatorMessage = new Int16[3];
        actuatorMessage[0] = (Int16)index;
        actuatorMessage[1] = (Int16)register;
        actuatorMessage[2] = length;
        SendInts(actuatorMessage, true);
    }

    public void SendCalibrationMessage(Actuator actuator)
    {
        int val = -30;
        Int16[] actuatorMessage = new Int16[3];
        actuatorMessage[0] = (Int16)actuator._ActuatorIndex;
        actuatorMessage[1] = (Int16)ModBusRegisters.MB_MOTOR_SPEED;
        actuatorMessage[2] = (Int16)val;
        SendInts(actuatorMessage, true);    
    }

    void SendHeartBeat()
    {
        print("Sending Heartbeat");
        SendInts(_HeartBeatMessage, true);
    }
}
