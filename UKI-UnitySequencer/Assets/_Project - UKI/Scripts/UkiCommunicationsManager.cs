using System.Collections;
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

    public void Calibrate()
    {

    }

    IEnumerator EStop()
    {
        print("Sending eStop");
        _EStopping = true;
        SendActuatorMessage((int)UkiTestActuatorAssignments.Global, 20560, ModBusRegisters.MB_ESTOP);
        yield return new WaitForSeconds(1.0f);

       SendActuatorMessage((int)UkiTestActuatorAssignments.Global, 20560, ModBusRegisters.MB_RESET_ESTOP);
        InvokeRepeating("SendHeartBeat", 1f, 1f);
        _EStopping = false;
    }

    private void Update()
    {
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
        
        /*
        if(_TestMode)
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                foreach(UkiActuatorAssignments assignment in _TestActuator)
                    StartCoroutine(SendSetSpeedThenStopAfterX(assignment, _TestSpeed, _TestTime));
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                foreach (UkiActuatorAssignments assignment in _TestActuator)
                    StartCoroutine(SendSetSpeedThenStopAfterX(assignment, -_TestSpeed, _TestTime));
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                foreach (UkiActuatorAssignments assignment in _TestActuator)
                    SendCalibrationMessage((int)assignment, -30);
            }
        }
        */
    }
  
    public void SendActuatorMessage(int index, int length, ModBusRegisters register)
    {
        uint[] actuatorMessage = new uint[3];
        actuatorMessage[0] = (uint)index;
        actuatorMessage[1] = (uint)register;
        actuatorMessage[2] = (uint)length;
        SendInts(actuatorMessage, true);
    }

    // Sends MB_GOTO_POSITION and MB_GOTO_SPEED_SETPOINT. Uses the inbuilt ramp to ramp up the motor speed
    // Max rated speed 30
    // Accel 0 - 100
    public void SendActuatorSetPointCommand(UkiActuatorAssignments actuator, int speed, int position, bool useBuiltInAccelRamp = true, int accel = 50)
    {
        if (useBuiltInAccelRamp)
        {
            // Set actuator length
            // Uses accel/speed ramp. Not sure why speed setpoint goto pos both need sending or if they do
            uint[] actuatorMessage2 = new uint[3];
            actuatorMessage2[0] = (uint)actuator;
            actuatorMessage2[1] = (uint)ModBusRegisters.MB_GOTO_POSITION;
            actuatorMessage2[2] = (uint)position;
            SendInts(actuatorMessage2, true);

            // Set speed with a ramp up. Try using without this to begin with if chris doesn't get back
            uint[] actuatorMessage = new uint[3];
            actuatorMessage[0] = (uint)actuator;
            actuatorMessage[1] = (uint)ModBusRegisters.MB_GOTO_SPEED_SETPOINT;
            actuatorMessage[2] = (uint)speed;
            SendInts(actuatorMessage, true);           
        }
        else
        {
            // Set actuator length
            // Uses instant accel, what ever the accel is last set too
            uint[] actuatorMessage = new uint[3];
            actuatorMessage[0] = (uint)actuator;
            actuatorMessage[1] = (uint)ModBusRegisters.MB_MOTOR_SETPOINT;
            actuatorMessage[2] = (uint)position;
            SendInts(actuatorMessage, true);

            // Set acceleration
            uint[] actuatorMessage2 = new uint[3];
            actuatorMessage2[0] = (uint)actuator;
            actuatorMessage2[1] = (uint)ModBusRegisters.MB_MOTOR_ACCEL;
            actuatorMessage2[2] = (uint)accel;
            SendInts(actuatorMessage2, true);
        }
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
        actuatorMessage1[2] = (uint)95; // accel
        SendInts(actuatorMessage1, true);

        uint[] actuatorMessage2 = new uint[3];
        actuatorMessage2[0] = (uint)index;
        actuatorMessage2[1] = (uint)ModBusRegisters.MB_MOTOR_SETPOINT;
        actuatorMessage2[2] = (uint)motorSpeed;
        SendInts(actuatorMessage2, true);

    }


    // Need to set a send a set speed
    public void SendPositionMessage(Actuator actuator)
    {
        uint[] actuatorMessage = new uint[3];
        actuatorMessage[0] = (uint)actuator._ActuatorIndex;
        actuatorMessage[1] = (uint)ModBusRegisters.MB_GOTO_POSITION;
        actuatorMessage[2] = (uint)actuator._CurrentLinearLength;
        SendInts(actuatorMessage, true);
    }

    public void SendPositionMessage(UkiActuatorAssignments actuatorAssignment, uint actuatorLength, uint speed)
    {
        uint[] actuatorMessage = new uint[3];
        actuatorMessage[0] = (uint)actuatorAssignment;
        actuatorMessage[1] = (uint)ModBusRegisters.MB_GOTO_POSITION;
        actuatorMessage[2] = (uint)actuatorLength;
        SendInts(actuatorMessage, true);
    }

    // Set speed for x seconds, needs to be used of actuators without encoders
    public void SendSetSpeedMessageForTime(Actuator actuator, float speed, float time)
    {
        StartCoroutine(SendSetSpeedThenStopAfterX(actuator._ActuatorIndex, speed, time));
    }

    IEnumerator SendSetSpeedThenStopAfterX(UkiActuatorAssignments actuatorAssign, float speed, float time)
    {
        print(actuatorAssign.ToString() + "  Setting speed too: " + speed + "     for x seconds: " + time);

        // Set speed
        uint[] actuatorMessage = new uint[3];
        actuatorMessage[0] = (uint)actuatorAssign;
        actuatorMessage[1] = (uint)ModBusRegisters.MB_MOTOR_SETPOINT; // only use for calibrate
        actuatorMessage[2] = (uint)speed;
        SendInts(actuatorMessage, true);

        yield return new WaitForSeconds(time);

        // Set speed to zero
        actuatorMessage[0] = (uint)actuatorAssign;
        actuatorMessage[1] = (uint)ModBusRegisters.MB_MOTOR_SETPOINT;
        actuatorMessage[2] = (uint)0;
        SendInts(actuatorMessage, true);

        print(actuatorAssign.ToString() + "  Setting speed too: 0");
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
