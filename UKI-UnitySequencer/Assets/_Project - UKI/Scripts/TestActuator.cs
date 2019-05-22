using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Basic actuator test that sets the position to move too
public class TestActuator : MonoBehaviour
{
    // State of the limb. Paused, Calibrating, Calibrated or Animating
    public UKIEnums.State _State = UKIEnums.State.Paused;

    // The index of the actuator
    public UkiActuatorAssignments _ActuatorIndex;

    public Vector3 _RotationAxis = Vector3.right;

    // Is the actuator calibrated back to 0
    public bool Calibrated { get { return _State == UKIEnums.State.CalibratedToZero; } }
    
    [Range(0, 1)]
    public float _NormExtension;

    [Range(0,30)]
    public float _Speed = 15;

    // ROTATION
    // Min and max rotation range
    public float _MinRotationInDegrees = 0;
    public float _MaxRotationInDegrees = 20;

    // LINEAR
    // Encoder extensions. Linear travel that gets converted into rotational movement
    float _CurrentEncoderExtension { get { return Mathf.Clamp(_NormExtension * _MaxEncoderExtension * 10, 0, _MaxEncoderExtension * 10); } }
    public float _MaxEncoderExtension = 40;

    public bool _SendOutPositionMessages = false;
    
    // Reported values from modbus
    public int _ReportedExtension;
    public int _ReportedSpeed;
    public int _ReportedAcceleration;
    
    private void Start()
    {
        UkiStateDB.RegisterActuator(_ActuatorIndex);
        StartCoroutine(SendPosAtRate(15));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
                SendEncoderExtensionLength();
        }

        // READ IN
        // Get the reported values from mod bus      
        _ReportedExtension = UkiStateDB._StateDB[_ActuatorIndex][ModBusRegisters.MB_EXTENSION];
        _ReportedSpeed = UkiStateDB._StateDB[_ActuatorIndex][ModBusRegisters.MB_GOTO_SPEED_SETPOINT];
        _ReportedAcceleration = UkiStateDB._StateDB[_ActuatorIndex][ModBusRegisters.MB_MOTOR_ACCEL];

        // UPDATE STATES
        if(_State == UKIEnums.State.Calibrating)
        {
            print(name + "    " + _ReportedExtension);
            if (_ReportedExtension == 0)
                SetState(UKIEnums.State.CalibratedToZero);
        }

        // Set the rotation from normalized extension
        float rot = _NormExtension.ScaleFrom01(_MinRotationInDegrees, _MaxRotationInDegrees);
        transform.localRotation = Quaternion.AngleAxis(rot, _RotationAxis);
    }
    public bool _DEBUG = false;
    public void SetState(UKIEnums.State state)
    {
        _State = state;

        if (_State == UKIEnums.State.Animating)
        {
        }
        else if (_State == UKIEnums.State.Calibrating)
        {
            SendCalibrateMessage();
        }

        print(name + " State set too: " + _State.ToString());
    }
    
    void SendEncoderExtensionLength()
    {
        UkiCommunicationsManager.Instance.SendActuatorSetPointCommand(_ActuatorIndex, (int)_CurrentEncoderExtension, (int)_Speed);
    }
    
    void SendCalibrateMessage()
    {
        UkiCommunicationsManager.Instance.SendCalibrationMessage((int)_ActuatorIndex, -30);      
    }
    
    IEnumerator SendPosAtRate(float ratePerSecond)
    {
        float wait = 1f / ratePerSecond;
        while (true)
        {
            if (_SendOutPositionMessages)
            {
                SendEncoderExtensionLength();
            }

            yield return new WaitForSeconds(wait);
        }
    }
}
