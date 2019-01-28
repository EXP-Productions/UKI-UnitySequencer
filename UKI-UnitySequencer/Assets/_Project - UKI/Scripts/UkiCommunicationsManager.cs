using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class UkiCommunicationsManager : ThreadedUDPReceiver
{
    public static int _CalibrateWaitTime = 60;

    private static uint[] _HeartBeatMessage = new uint[] {240, 0, 0};

    public static UkiCommunicationsManager Instance {  get { return _Instance; } }
    private static UkiCommunicationsManager _Instance;

    public bool _EStopping = true;

    public override void Awake()
    {
        base.Awake();
        _Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("EStop");
    }

    IEnumerator EStop()
    {
        UkiCommunicationsManager.Instance.SendActuatorMessage((int)UkiTestActuatorAssignments.Global, 20560, ModBusRegisters.MB_RESET_ESTOP);
        yield return new WaitForSeconds(1.0f);

        UkiCommunicationsManager.Instance.SendActuatorMessage((int)UkiTestActuatorAssignments.Global, 20560, ModBusRegisters.MB_RESET_ESTOP);
        InvokeRepeating("SendHeartBeat", 1f, 1f);
        _EStopping = false;
    }

    private void Update()
    {
        while(_ReceivedPackets.Count > 0)
        {
            byte[] packet = _ReceivedPackets.Dequeue();

            //int actuatorIndex = System.BitConverter.ToInt16(packet, 0);
            int actuatorIndex = GetLittleEndianIntegerFromByteArray(packet, 0);

            for (int i = 2; i < packet.Length; i+=4)
            {
                int registerIndex = GetLittleEndianIntegerFromByteArray(packet, i);
                int registerValue = GetLittleEndianIntegerFromByteArray(packet,  i+2);

                print("Actuator: " + actuatorIndex + ", register: " + registerIndex + ", value: " + registerValue);
                if (UkiStateDB._StateDB.ContainsKey(actuatorIndex))
                {
                    UkiStateDB._StateDB[actuatorIndex][registerIndex] = registerValue;
                }
            }
        }        
    }

    int GetLittleEndianIntegerFromByteArray(byte[] data, int startIndex)
    {
        return (data[startIndex + 1] << 8)
             | data[startIndex];
    }

    public void SendPositionMessage(Actuator actuator)
    {
        uint[] actuatorMessage = new uint[3];
        actuatorMessage[0] = (uint)actuator._ActuatorIndex;
        actuatorMessage[1] = (uint)ModBusRegisters.MB_GOTO_POSITION;
        actuatorMessage[2] = (uint)actuator._CurrentLinearLength;
        SendInts(actuatorMessage, true);
    }

    public void SendActuatorMessage(int index, int length, ModBusRegisters register)
    {
        uint[] actuatorMessage = new uint[3];
        actuatorMessage[0] = (uint)index;
        actuatorMessage[1] = (uint)register;
        actuatorMessage[2] = (uint)length;
        SendInts(actuatorMessage, true);
    }

    public void SendCalibrationMessage(Actuator actuator, int motorSpeed)
    {
        uint[] actuatorMessage = new uint[3];
        actuatorMessage[0] = (uint)actuator._ActuatorIndex;
        actuatorMessage[1] = (uint)ModBusRegisters.MB_MOTOR_SPEED;
        actuatorMessage[2] = (uint)motorSpeed;
        SendInts(actuatorMessage, true);
    }

    public void SendCalibrationMessage(int index, int motorSpeed)
    {
        uint[] actuatorMessage1 = new uint[3];
        actuatorMessage1[0] = (uint)index;
        actuatorMessage1[1] = (uint)ModBusRegisters.MB_MOTOR_ACCEL;
        actuatorMessage1[2] = (uint)95;
        SendInts(actuatorMessage1, true);

        uint[] actuatorMessage2 = new uint[3];
        actuatorMessage2[0] = (uint)index;
        actuatorMessage2[1] = (uint)ModBusRegisters.MB_MOTOR_SETPOINT;
        actuatorMessage2[2] = (uint)motorSpeed;
        SendInts(actuatorMessage2, true);

    }

    void SendHeartBeat()
    {
        //print("Sending Heartbeat");
        SendInts(_HeartBeatMessage, true);
    }
}
