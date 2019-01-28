using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

/// <summary>
/// Actuators are driven by the animations on the timeline
/// It sets the real world actuator length by interpretting the rotation into a linear value
/// </summary>
[System.Serializable]
[RequireComponent(typeof(RotationLimitHinge))]
public class Actuator : MonoBehaviour
{

    /// <summary>
    /// UKI Angles
    /// Front left ankle -length out 77     Out 140 In 100
    /// Front left Knee - length out 84     Out 192 In 136 measured from top of hip
    /// Front left Hip - length out 75      Out 100 In 50 measured on inside of hexagon from cross bar
    /// </summary>

        /// Speed is a percent of voltage
        /// mm per second per volt    57mm over 15.5 seconds

    public UkiActuatorAssignments _ActuatorIndex = 0;

    bool _Calibrated = false;
    public bool Calibrated { get { return _Calibrated; } }

    public float _MaxLinearTravel = 50;       // Maximum that that linear actuator can travel, in mm
    public float _CurrentLinearLength = 0;    // Current length that the linear actuator is at
    public float _TargetLinearLength = 0;    // Target length that the linear actuator is aiming for
    float _PrevLinearLength = 0;
    float _LinearLengthMessageSendCutoff = 1;

    Quaternion _TargetRotation;
    
    public float _RotationBase = 0;         // Roation of the joint when it is zeroed/calibrated
    public float _RotationExtended = 30;    // Roation of the joint when it is full extended

    float _RotationTargetAngle = 0;     // Target angle for roation
    float _RotationCurrentAngle = 0;    // Currecnt angle for rotation
    
    RotationLimitHinge _RotationLimitHinge; // Limit hinge used in the IK. Used to set the min and max angles
    public UKILimb _ParentLimb;

    public Transform _RealWorldProxy;

    public void Init(UKILimb parentLimb)
    {
        _ParentLimb = parentLimb;
        _RotationLimitHinge = GetComponent<RotationLimitHinge>();
        _PrevLinearLength = _CurrentLinearLength;
    }

    private void Awake()
    {
        UkiStateDB.RegisterActuator(this);
    }

    private void Update()
    {
        if(_ParentLimb._State == UKIEnums.State.Calibrating)
        {
            Quaternion targetRot = _RotationLimitHinge.defaultLocalRotation;
            //transform.localRotation = targetRot;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * 4);
            _RotationLimitHinge.Apply();

            float angle = Quaternion.Angle(transform.localRotation, targetRot);

            print(name + " angle: " + angle);
            
            _Calibrated = angle < 1;
        }

        if (_RealWorldProxy != null)
            _RealWorldProxy.transform.localRotation = Quaternion.Slerp(_RealWorldProxy.transform.localRotation, transform.localRotation, Time.deltaTime * 3);
    }

    // read encoder linear value and convert to rotation
    void ReadInEncoader()
    {
        
    }

    void UpdateRotation()
    {
        // Add any smoothing and accel stuff in here and limits

        // Limit roation to base and extended
        _RotationCurrentAngle = Mathf.Clamp(_RotationCurrentAngle, _RotationBase, _RotationExtended);


        if (Mathf.Abs(_CurrentLinearLength - _PrevLinearLength) > _LinearLengthMessageSendCutoff)
        {
            // Send out UDP here
            UkiCommunicationsManager.Instance.SendPositionMessage(this);
        }
    }
    
    public void CalibrateToZero()
    {
        print("Setting too: " + _RotationLimitHinge.defaultLocalRotation.eulerAngles.ToString());

        transform.localRotation = _RotationLimitHinge.defaultLocalRotation;
        _RotationLimitHinge.Apply();
        StartCoroutine(Calibrate());
    }

    IEnumerator Calibrate()
    {
        while (UkiCommunicationsManager.Instance._EStopping)
        {
            yield return new WaitForSeconds(1.0f);
        }

        UkiCommunicationsManager.Instance.SendCalibrationMessage((int)this._ActuatorIndex, -55);
        yield return new WaitForSeconds(UkiCommunicationsManager._CalibrateWaitTime);
        print("done sending calibration messages");
        SetCalibrated();
    }

    public void CalibrateToMax()
    {
        UkiCommunicationsManager.Instance.SendCalibrationMessage(this, 30);
    }


    void SetCalibrated()
    {
        _Calibrated = true;
    }

    public void OnCalibrationCompleteHandler()
    {

    }
}