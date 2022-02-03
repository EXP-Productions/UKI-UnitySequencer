using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public enum UKIMode
{
    Simulation,
    SendUDP,
    SendTCP,    
}


// Time stamp - Using system time and time from start
// Add send recieve counts
// Send recieve over time in SR debugger
// Add anlge and and extension into the debug


/// Comms manager for UKI
/// Sends out the commands that come in from the actuators
public class UkiCommunicationsManager : ThreadedUDPReceiver
{
    // Singleton
    public static UkiCommunicationsManager Instance { get { return _Instance; } }
    private static UkiCommunicationsManager _Instance;

    [HideInInspector]
    public UKI_UIManager _UIManager;

    public Client _TCPClient;

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

    public bool _DBInitialized = false;

    // Send the messages out to modbus to set the real world limbs positions
    public UKIMode _UKIMode = UKIMode.Simulation;
    public bool IsSimulating { get { return _UKIMode == UKIMode.Simulation; } }

    // 20 0 65
    // -30 -10 85
    // 55 20 77
    // Sends MB_GOTO_POSITION and MB_GOTO_SPEED_SETPOINT. Uses the inbuilt ramp to ramp up the motor speed
    // Max rated speed 30
    // Accel 0 - 100
    int _SentMsgCount = 0;
    float _Timer = 0;
    public float msgPerSec = 0;
    float lastTime = 0;

    [Header("DEBUG")]
    public bool _DebugActuatorInternal = false;
    public bool _DebugSend = false;
    public bool _DebugRecieve = false;
    public bool _DebugUDP = false;

    int _PacketCounter = 0;

    public Server _TCPServer;

    // Set singleton instance and start Threaded UDP reciever
    public override void Awake()
    {
        base.Awake();
        _Instance = this;

        Server.onHandleByteData += WriteData;
    }

    void OnApplicationQuit()
    {
        Server.onHandleByteData -= WriteData;
    }

    void Start()
    {
        _Actuators = FindObjectsOfType<Actuator>();
        _UIManager = UKI_UIManager.Instance;

        StartCoroutine(StartupProcedure());
        StartCoroutine(SendHeartBeat());
    }

    public void SetUKIMode(int i)
    {
        print("UKI Mode set too: " + i);

        _UKIMode = (UKIMode)i;

        if (_UKIMode == UKIMode.SendUDP)
        {
            StartCoroutine(StartupProcedure());
        }
        else if (_UKIMode == UKIMode.SendTCP)
        {
            print("SendTCP");
            StartCoroutine(StartupProcedure());
            _TCPClient.ConnectToServer();
        }
        else if (_UKIMode == UKIMode.Simulation)
        {
            EStop("Switching to simulation");
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
        if (_UIManager == null)
            _UIManager = UKI_UIManager.Instance;

        _UIManager._EstopWarning.SetActive(true);
        _UIManager._EstopWarning.GetComponentInChildren<TextMeshProUGUI>().text = reason;
        print("E Stop activated: " + reason);
        _EStopping = true;
        _UIManager.UpdateEstopButton();
        
        if (IsSimulating)
            return;

        // Send message to wrapper
        uint[] actuatorMessage = new uint[3];
        actuatorMessage[0] = (uint)UkiTestActuatorAssignments.Global;
        actuatorMessage[1] = (uint)ModBusRegisters.MB_ESTOP;
        actuatorMessage[2] = (uint)20560;

        if(_UKIMode == UKIMode.SendUDP)
            SendInts(actuatorMessage, true);
        else
            _TCPClient.WriteInts(actuatorMessage, true);
    }
    
    void ResetEStop()
    {
        print("Resetting estop");

        //SendActuatorMessage((int)UkiTestActuatorAssignments.Global, ModBusRegisters.MB_RESET_ESTOP);

        if (!IsSimulating)
        {
            // Send message to wrapper
            uint[] actuatorMessage = new uint[3];
            actuatorMessage[0] = (uint)UkiTestActuatorAssignments.Global;
            actuatorMessage[1] = (uint)ModBusRegisters.MB_RESET_ESTOP;
            actuatorMessage[2] = (uint)20560;

            if (_UKIMode == UKIMode.SendUDP)
                SendInts(actuatorMessage, true);
            else
                _TCPClient.WriteInts(actuatorMessage, true);
        }

        _EStopping = false;
        _UIManager.UpdateEstopButton();
        foreach (GameObject collisionMarker in GameObject.FindGameObjectsWithTag(SRTags.CollisionMarker))
        {
            Destroy(collisionMarker);
        }

        foreach (KeyValuePair<UkiActuatorAssignments, Actuator> actuator in UKI_UIManager.Instance._AllActuators)
            actuator.Value.ResetEStop();

        _UIManager._EstopWarning.SetActive(false);
    }

    IEnumerator StartupProcedure()
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
             

        // Try moving to Fixed update
        ReceiveStateData();
    }

    int _RecieveCount;
    public void ReceiveStateData()
    {
        if (_DebugRecieve && _ReceivedPackets.Count > 0)
        {
            _RecieveCount++;
            Debug.LogWarning("Packets recieved: " + _ReceivedPackets.Count + "  Time:  " + _Timer + "   recieve count: " + _RecieveCount);           
        }

        while (_ReceivedPackets.Count > 0)
        {
            // Get packet
            // First 2 bytes - Actuator index second 2 bytes - register index Next 2 bytes - reg value 
            // Total 6 bytes
            byte[] packet = _ReceivedPackets.Dequeue();

            if (_DebugRecieve)
            {
                for (int i = 0; i < packet.Length; i++)
                {
                    Debug.Log($"Packet {i}   ");
                }
            }


            // Get actuator index
            int actuatorIndex = GetLittleEndianIntegerFromByteArray(packet, 0);

            _PacketCounter++;

            // Start at index 2 (TODO why steeb?) and iterate with a step of 4
            for (int i = 2; i < packet.Length; i += 4)
            {
                // get index and value for register
                int registerIndex = GetLittleEndianIntegerFromByteArray(packet, i);
                int registerValue = GetLittleEndianIntegerFromByteArray(packet, i + 2);

                // Get actuator enum
                UkiActuatorAssignments actuatorEnum = (UkiActuatorAssignments)actuatorIndex;
                ModBusRegisters registerEnum = (ModBusRegisters)registerIndex;


                if (_DebugRecieve)
                {
                    Debug.LogWarning($"Packet recieved {_PacketCounter} {Time.time}  Actuator {actuatorEnum.ToString()}   Reg Index {registerIndex}  Reg Enum {registerEnum.ToString()}   Reg Value {registerValue}");
                }

                if (UkiStateDB._StateDB.ContainsKey(actuatorEnum))
                {
                    UkiStateDB._StateDB[actuatorEnum][registerEnum] = registerValue;
                }
            }

            _DBInitialized = true;
        }
    }

    public void WriteData(byte[] packet)
    {      
        // Get actuator index
        int actuatorIndex = GetLittleEndianIntegerFromByteArray(packet, 0);

        // Start at index 2 (TODO why steeb?) and iterate with a step of 4
        for (int i = 2; i < packet.Length; i += 4)
        {
            // get index and value for register
            int registerIndex = GetLittleEndianIntegerFromByteArray(packet, i);
            int registerValue = GetLittleEndianIntegerFromByteArray(packet, i + 2);

            // Get actuator enum
            UkiActuatorAssignments actuatorEnum = (UkiActuatorAssignments)actuatorIndex;
            ModBusRegisters registerEnum = (ModBusRegisters)registerIndex;

            if (_DebugRecieve)
            {
                Debug.LogWarning($"{Time.timeSinceLevelLoad}  Packet recieved {Time.time}  Actuator {actuatorEnum.ToString()}   Reg Index {registerIndex}  Reg Enum {registerEnum.ToString()}   Reg Value {registerValue}");
            }

            if (UkiStateDB._StateDB.ContainsKey(actuatorEnum))// && UkiStateDB._StateDB[actuatorEnum][registerEnum] != registerValue)
            {                
                UkiStateDB._StateDB[actuatorEnum][registerEnum] = registerValue;
            }
        }

        _DBInitialized = true;
    }

    public void SendActuatorSetPointCommand(UkiActuatorAssignments actuator, int position, int speed = 10)
    {
        if (IsSimulating)
            return;
        
        if (_DebugSend)
            Debug.Log(" ----- Sending: Setting encoder: " + actuator.ToString() + " too pos: " + position + " speed: " + speed + "     Time:" + _Timer );

        speed = Mathf.Clamp(speed, 0, 30);
        //position = Mathf.Clamp(speed, 0, 100);

        // Set actuator position
        uint[] actuatorPosMsg = new uint[3];
        actuatorPosMsg[0] = (uint)actuator;
        actuatorPosMsg[1] = (uint)ModBusRegisters.MB_GOTO_POSITION;
        actuatorPosMsg[2] = (uint)position;
        if (_UKIMode == UKIMode.SendUDP)
            SendInts(actuatorPosMsg, true);
        else
            _TCPClient.WriteInts(actuatorPosMsg, true);

        // Set speed
        uint[] actuatorSpeedMsg = new uint[3];
        actuatorSpeedMsg[0] = (uint)actuator;
        actuatorSpeedMsg[1] = (uint)ModBusRegisters.MB_GOTO_SPEED_SETPOINT;
        actuatorSpeedMsg[2] = (uint)speed;
        if (_UKIMode == UKIMode.SendUDP)
            SendInts(actuatorSpeedMsg, true);
        else
            _TCPClient.WriteInts(actuatorSpeedMsg, true);

        // TODO - DEBUG ADDING PADDING
        if (_UKIMode == UKIMode.SendUDP)
            SendInts(_HeartBeatMessage, true);
        else
            _TCPClient.WriteInts(_HeartBeatMessage, true);


        _SentMsgCount++;

        if(lastTime == 0)
        {
            lastTime = Time.time;
        }
        else
        {

        }

        if (_DebugSend)
        {
            msgPerSec = _SentMsgCount / _Timer;
            Debug.LogWarning("Total msg count: " + _SentMsgCount + "   Total run time: " + _Timer);
        }
    }
    
    // Sends a message to set actuator accel to 90 and setpoint 0 
    public void SendCalibrationMessage(int index, int motorSpeed)
    {
        if (IsSimulating)
            return;

        uint[] actuatorMessage1 = new uint[3];
        actuatorMessage1[0] = (uint)index;
        actuatorMessage1[1] = (uint)ModBusRegisters.MB_MOTOR_ACCEL;
        actuatorMessage1[2] = (uint)95; // accel
        if (_UKIMode == UKIMode.SendUDP)
            SendInts(actuatorMessage1, true);
        else
            _TCPClient.WriteInts(actuatorMessage1, true);

        uint[] actuatorMessage2 = new uint[3];
        actuatorMessage2[0] = (uint)index;
        actuatorMessage2[1] = (uint)ModBusRegisters.MB_MOTOR_SETPOINT;
        actuatorMessage2[2] = (uint)motorSpeed;
        if (_UKIMode == UKIMode.SendUDP)
            SendInts(actuatorMessage2, true);
        else
            _TCPClient.WriteInts(actuatorMessage2, true);
    }

    /*
    public void SendActuatorMessage(int index, ModBusRegisters register)
    {
        if (_UKIMode == UKIMode.Simulation)
            return;

        uint[] actuatorMessage = new uint[3];
        actuatorMessage[0] = (uint)index;
        actuatorMessage[1] = (uint)register;
        actuatorMessage[2] = (uint)20560;
        SendInts(actuatorMessage, true);
    }
    */

    IEnumerator SendHeartBeat()
    {
        while (true)
        {
            if (!IsSimulating && !_EStopping)
            {
                yield return new WaitForSeconds(0.5f);
                _UIManager._HeartBeatDisplay.color = Color.red;
                
                if (_UKIMode == UKIMode.SendUDP)
                    SendInts(_HeartBeatMessage, true);
                else if (_UKIMode == UKIMode.SendTCP)
                    _TCPClient.WriteInts(_HeartBeatMessage, true);

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
        return (data[startIndex + 1] << 8) | data[startIndex];
    }

}
