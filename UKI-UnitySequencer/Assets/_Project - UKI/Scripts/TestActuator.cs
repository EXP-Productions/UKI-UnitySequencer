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
    // A copy of the transform that is driven from the read in value
    public Transform _ReportedActuatorTransform;


    #region LINEAR EXTENSION
    [Header("LINEAR EXTENSION")]
    // Actuator extension. Linear travel that gets converted into rotational movement   
    [Range(0, 1)]
    // Normalized extension value
    public float    _NormExtension;
    // Maximum value the encoder can be extended too  
    public float    _MaxEncoderExtension = 40;
    [Range(0, 30)]
    // Speed to send commands at
    public int _ExtensionSpeed = 30;
    // The current encoder extension is scaled by 10 because modbus is expecting a mm value with a decimal place
    float           CurrentEncoderExtension { get { return Mathf.Clamp(_NormExtension * _MaxEncoderExtension * 10, 0, _MaxEncoderExtension * 10); } }   
    #endregion


    #region ROTATION
    [Header("ROTATION MAPPING")]
    // Min and max rotation range
    public Vector3  _RotationAxis = Vector3.right;
    public Vector3  _ForwardAxis = Vector3.up;
    public float    _MinRotationInDegrees = 0;
    public float    _MaxRotationInDegrees = 20;
    float RotationRange { get { return _MaxRotationInDegrees - _MinRotationInDegrees; } }

    Quaternion _InitialRotation;

    #endregion


    #region MODBUS
    [Header("MODBUS")]
    // Send the messages out to modbus to set the real world limbs positions
    public bool _SendToModbus = false;    
    // Extension value read in from modbus - Doesn't line up exactly with the set point value we send
    public int _ReportedExtension;
    // The maximum extension reported when extended to the full length of _MaxEncoderExtension
    public float _MaxReportedExtension = 40;    
    // Reporeted speed and accel read in from modbus
    public int ReportedSpeed { private set; get; }
    public int ReportedAcceleration { private set; get; }
    #endregion
    

    private void Start()
    {
        // Register the actuator
        UkiStateDB.RegisterActuator(_ActuatorIndex);

        // Start the send position coroutine
        StartCoroutine(SendPosAtRate(15));

        // Set names of the actuators and the reported actuator transforms
        name = "Actuator - " + _ActuatorIndex.ToString(); 
        _ReportedActuatorTransform.name = "REPORTED - " + name;

        _InitialRotation = transform.localRotation;
    }

    [ContextMenu("Set rotation axis")]
    public void SetRotationAxis()
    {
        // Set rotation axis from local right axis
        _RotationAxis = transform.TransformDirection(_RotationAxis);
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
        ReportedSpeed = UkiStateDB._StateDB[_ActuatorIndex][ModBusRegisters.MB_GOTO_SPEED_SETPOINT];
        ReportedAcceleration = UkiStateDB._StateDB[_ActuatorIndex][ModBusRegisters.MB_MOTOR_ACCEL];

        // UPDATE STATES
        if(_State == UKIEnums.State.Calibrating)
        {
            print(name + "    " + _ReportedExtension);
            if (_ReportedExtension == 0)
                SetState(UKIEnums.State.CalibratedToZero);
        }

        // Set the rotation from normalized extension
        //float rot = _NormExtension.ScaleFrom01(_MinRotationInDegrees, _MaxRotationInDegrees); 
        float rot = _NormExtension.ScaleFrom01(0, RotationRange);
        transform.localRotation = _InitialRotation * Quaternion.AngleAxis(rot, _RotationAxis);

        // Update the readin actuator transform
        float normReportedExtension = (float)_ReportedExtension / _MaxReportedExtension;
        float reportedRotation = normReportedExtension.ScaleFrom01(0, RotationRange);
        _ReportedActuatorTransform.localRotation = _InitialRotation * Quaternion.AngleAxis(reportedRotation, _RotationAxis);
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

    // Is the actuator calibrated back to 0
    public bool IsCalibrated()
    {
        return _State == UKIEnums.State.CalibratedToZero;
    }


    void SendEncoderExtensionLength()
    {
        UkiCommunicationsManager.Instance.SendActuatorSetPointCommand(_ActuatorIndex, (int)CurrentEncoderExtension, (int)_ExtensionSpeed);
    }
    
    void SendCalibrateMessage()
    {
        UkiCommunicationsManager.Instance.SendCalibrationMessage((int)_ActuatorIndex, -(int)_ExtensionSpeed);      
    }
    
    IEnumerator SendPosAtRate(float ratePerSecond)
    {
        float wait = 1f / ratePerSecond;
        while (true)
        {
            if (_SendToModbus)
            {
                SendEncoderExtensionLength();
            }

            yield return new WaitForSeconds(wait);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.TransformPoint(-_RotationAxis * .3f), transform.TransformPoint(_RotationAxis * .3f));
       
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.TransformPoint(_ForwardAxis * .3f));
    }
}
