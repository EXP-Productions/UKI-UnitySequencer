﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public enum UKIMode
{
    SendUDP,
    Simulation,
}

/// Comms manager for UKI
/// Sends out the commands that come in from the actuators
public class UkiCommunicationsManager : ThreadedUDPReceiver
{
    public static UkiCommunicationsManager Instance { get { return _Instance; } }
    private static UkiCommunicationsManager _Instance;

    [HideInInspector]
    public UKI_UIManager _UIManager;

    Actuator[] _Actuators;

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

    // Send the messages out to modbus to set the real world limbs positions
    public UKIMode _UKIMode = UKIMode.Simulation;

    public bool _Debug = false;

    public override void Awake()
    {
        base.Awake();
        _Instance = this;       
    }

    void Start()
    {
        _Actuators = FindObjectsOfType<Actuator>();
        _UIManager = UKI_UIManager.Instance;

        StartCoroutine(UDPStartupProcedure());

        StartCoroutine(SendHeartBeat());
    }

    public void SetUKIMode(int i)
    {
        print("UKI Mode set too: " + i);

        _UKIMode = (UKIMode)i;

        if(_UKIMode == UKIMode.SendUDP)
        {
            StartCoroutine(UDPStartupProcedure());
        }
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
        _UIManager._EstopWarning.SetActive(true);
        _UIManager._EstopWarning.GetComponentInChildren<TextMeshProUGUI>().text = reason;
        print("E Stop activated: " + reason);
        _EStopping = true;
        _UIManager.UpdateEstopButton();
        SendActuatorMessage((int)UkiTestActuatorAssignments.Global, 20560, ModBusRegisters.MB_ESTOP);
    }
    
    void ResetEStop()
    {
        print("Resetting estop");
        SendActuatorMessage((int)UkiTestActuatorAssignments.Global, 20560, ModBusRegisters.MB_RESET_ESTOP);
      
        _EStopping = false;
        _UIManager.UpdateEstopButton();
        foreach (GameObject collisionMarker in GameObject.FindGameObjectsWithTag(SRTags.CollisionMarker))
        {
            Destroy(collisionMarker);
        }

        foreach (Actuator act in UKI_UIManager.Instance._AllActuators)
            act.ResetEStop();

        _UIManager._EstopWarning.SetActive(false);
    }

    IEnumerator UDPStartupProcedure()
    {
        // Send estop at start
        EStop("Start E Stop");

        // TODO don't base this on time alone
        yield return new WaitForSeconds(1f);
      
        foreach (Actuator actuator in _Actuators)
        {
            actuator.SetToReportedExtensionOnStartup();
        }
        
        yield return new WaitForSeconds(1f);

        ResetEStop();

        UKI_UIManager.Instance.SetActuatorSliders();
    }

    private void Update()
    {
        _Timer += Time.deltaTime;

        if(_Timer >= 10)
        {
            _Timer -= 10;
            msgCount = 0;
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
    int msgCount = 0;
    float _Timer = 0;
    public float msgPerSec = 0;
    float lastTime = 0;
    public void SendActuatorSetPointCommand(UkiActuatorAssignments actuator, int position, int speed = 10)
    {
        if (_UKIMode == UKIMode.Simulation)
            return;

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

        msgCount++;

        if(lastTime == 0)
        {
            lastTime = Time.time;
        }
        else
        {

        }

        msgPerSec = (msgCount / _Timer) * .1f;
        //print("Messages per second: " + msgPerSec);
    }
    
    // Only used on joints without actuators
    public void SendCalibrationMessage(int index, int motorSpeed)
    {
        if (_UKIMode == UKIMode.Simulation)
            return;

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
        if (_UKIMode == UKIMode.Simulation)
            return;

        uint[] actuatorMessage = new uint[3];
        actuatorMessage[0] = (uint)index;
        actuatorMessage[1] = (uint)register;
        actuatorMessage[2] = (uint)length;
        SendInts(actuatorMessage, true);
    }

    IEnumerator SendHeartBeat()
    {
        while (true)
        {
            if (_UKIMode == UKIMode.SendUDP && !_EStopping)
            {
                yield return new WaitForSeconds(0.5f);
                _UIManager._HeartBeatDisplay.color = Color.red;                
                SendInts(_HeartBeatMessage, true);
                yield return new WaitForSeconds(0.5f);
                _UIManager._HeartBeatDisplay.color = Color.white;
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }


    int GetLittleEndianIntegerFromByteArray(byte[] data, int startIndex)
    {
        return (data[startIndex + 1] << 8)
             | data[startIndex];
    }
}
