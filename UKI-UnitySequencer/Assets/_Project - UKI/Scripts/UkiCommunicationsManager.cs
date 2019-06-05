﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// Comms manager for UKI
/// Sends out the commands that come in from the actuators
public class UkiCommunicationsManager : ThreadedUDPReceiver
{
    public static UkiCommunicationsManager Instance { get { return _Instance; } }
    private static UkiCommunicationsManager _Instance;

    // Calibration wait time, this is how long we wait for all limbs to come in before we consider them calibrated
    public static float _CalibrateWaitTime = 60f;

    // Heart beat message that gets sent out to UKI
    private static uint[] _HeartBeatMessage = new uint[] {240, 0, 0};
    
    // Is UKI estopping, preventing all the limbs from moving
    public bool _EStopping = true;
    
    public UkiActuatorAssignments[] _TestActuator;
    public float _TestSpeed = 1;
    public float _TestTime = 1;

    public float _TestPos = 100;

    public bool _Debug = false;

    public override void Awake()
    {
        base.Awake();
        _Instance = this;
    }

    void Start()
    {
        EStop("Start E Stop");
        StartCoroutine(SetReportedExtensions());
    }

    public void Calibrate()
    {

    }

    public void EStopButtonToggle()
    {
        if (_EStopping)
            ResetEStop();
        else
            EStop("Button press");
    }

    public void EStop(string reason)
    {
        print("E Stop activated: " + reason);
        _EStopping = true;
        FindObjectOfType<UKI_UIManager>().UpdateEstopButton();
        SendActuatorMessage((int)UkiTestActuatorAssignments.Global, 20560, ModBusRegisters.MB_ESTOP);
        CancelInvoke("SendHeartBeat");
    }
    
    void ResetEStop()
    {
        SendActuatorMessage((int)UkiTestActuatorAssignments.Global, 20560, ModBusRegisters.MB_RESET_ESTOP);
        CancelInvoke("SendHeartBeat");
        InvokeRepeating("SendHeartBeat", 1f, 1f);
        _EStopping = false;
        FindObjectOfType<UKI_UIManager>().UpdateEstopButton();
        foreach (GameObject collisionMarker in GameObject.FindGameObjectsWithTag(SRTags.CollisionMarker))
        {
            Destroy(collisionMarker);
        }
    }

    IEnumerator SetReportedExtensions()
    {
        // TODO don't base this on time alone
        yield return new WaitForSeconds(1f);
        foreach (TestActuator actuator in FindObjectsOfType<TestActuator>())
        {
            actuator.SetToReportedExtension();
        }
        yield return new WaitForSeconds(1f);
        ResetEStop();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            EStop("Button press");
        }
        
        ReceiveStateData();
        //while(_ReceivedPackets.Count > 0)
        //{
        //    byte[] packet = _ReceivedPackets.Dequeue();

        //    //int actuatorIndex = System.BitConverter.ToInt16(packet, 0);
        //    int actuatorIndex = GetLittleEndianIntegerFromByteArray(packet, 0);

        //    for (int i = 2; i < packet.Length; i+=4)
        //    {
        //        int registerIndex = GetLittleEndianIntegerFromByteArray(packet, i);
        //        int registerValue = GetLittleEndianIntegerFromByteArray(packet,  i+2);
        //        if(registerIndex == 299)
        //            //print("Actuator: " + actuatorIndex + ", Extension :" + registerValue);
        //        if (registerIndex == 303)
        //            //print("Actuator: " + actuatorIndex + ", MB_INWARD_ENDSTOP_STATE :" + registerValue);
        //        if (UkiStateDB._StateDB.ContainsKey(actuatorIndex))
        //        {
        //            UkiStateDB._StateDB[actuatorIndex][registerIndex] = registerValue;
        //        }
        //    }
        //}  
    }

    public bool _DBInitialized = false;
    public void ReceiveStateData()
    {
        while (_ReceivedPackets.Count > 0)
        {
            byte[] packet = _ReceivedPackets.Dequeue();

            //int actuatorIndex = System.BitConverter.ToInt16(packet, 0);
            int actuatorIndex = GetLittleEndianIntegerFromByteArray(packet, 0);

            for (int i = 2; i < packet.Length; i += 4)
            {
                int registerIndex = GetLittleEndianIntegerFromByteArray(packet, i);
                int registerValue = GetLittleEndianIntegerFromByteArray(packet, i + 2);

                UkiActuatorAssignments actuatorEnum = (UkiActuatorAssignments)actuatorIndex;
                ModBusRegisters registerEnum = (ModBusRegisters)registerIndex;

                if (UkiStateDB._StateDB.ContainsKey(actuatorEnum))
                {
                    UkiStateDB._StateDB[actuatorEnum][registerEnum] = registerValue;
                }
            }

            _DBInitialized = true;
        }
    }

    // 20 0 65
    // -30 -10 85
    // 55 20 77
    // Sends MB_GOTO_POSITION and MB_GOTO_SPEED_SETPOINT. Uses the inbuilt ramp to ramp up the motor speed
    // Max rated speed 30
    // Accel 0 - 100
    public void SendActuatorSetPointCommand(UkiActuatorAssignments actuator, int position, int speed = 10)
    {
        if(_Debug)
            print("Setting encoder: " + actuator.ToString() + " too pos: " + position + " speed: " + speed);

        speed = Mathf.Clamp(speed, 0, 30);
        //position = Mathf.Clamp(speed, 0, 100);

        // Set speed
        uint[] actuatorMessage = new uint[3];
        actuatorMessage[0] = (uint)actuator;
        actuatorMessage[1] = (uint)ModBusRegisters.MB_GOTO_SPEED_SETPOINT;
        actuatorMessage[2] = (uint)speed;
        SendInts(actuatorMessage, true);

        // Set actuator position
        uint[] actuatorMessage2 = new uint[3];
        actuatorMessage2[0] = (uint)actuator;
        actuatorMessage2[1] = (uint)ModBusRegisters.MB_GOTO_POSITION;
        actuatorMessage2[2] = (uint)position;
        SendInts(actuatorMessage2, true);
    }
    
    // Only used on joints without actuators
    public void SendCalibrationMessage(int index, int motorSpeed)
    {
        uint[] actuatorMessage1 = new uint[3];
        actuatorMessage1[0] = (uint)index;
        actuatorMessage1[1] = (uint)ModBusRegisters.MB_MOTOR_ACCEL;
        actuatorMessage1[2] = (uint)95; // accel
        SendInts(actuatorMessage1, true);

        uint[] actuatorMessage2 = new uint[3];
        actuatorMessage2[0] = (uint)index;
        actuatorMessage2[1] = (uint)ModBusRegisters.MB_MOTOR_SETPOINT;
        actuatorMessage2[2] = (uint)motorSpeed;
        SendInts(actuatorMessage2, true);
    }

    public void SendActuatorMessage(int index, int length, ModBusRegisters register)
    {
        uint[] actuatorMessage = new uint[3];
        actuatorMessage[0] = (uint)index;
        actuatorMessage[1] = (uint)register;
        actuatorMessage[2] = (uint)length;
        SendInts(actuatorMessage, true);
    }
    
    void SendHeartBeat()
    {
        //print("Sending Heartbeat");
        SendInts(_HeartBeatMessage, true);
    }
    
    int GetLittleEndianIntegerFromByteArray(byte[] data, int startIndex)
    {
        return (data[startIndex + 1] << 8)
             | data[startIndex];
    }
}
