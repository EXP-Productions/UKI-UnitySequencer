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
    public UkiActuatorAssignments _ActuatorIndex = 0;

    bool _Calibrated = false;
    public bool Calibrated { get { return _Calibrated; } }

    public float _MaxLinearTravel = 50;       // Maximum that that linear actuator can travel
    public float _CurrentLinearLength = 0;    // Current length that the linear actuator is at
    public float _TargetLinearLength = 0;    // Target length that the linear actuator is aiming for

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
    }

    private void Update()
    {
        if(_ParentLimb._State == UKILimb.State.Calibrating)
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

    /*
    public void CalibrateToZero()
    {
        print("Setting too: " + _RotationLimitHinge.defaultLocalRotation.eulerAngles.ToString());

        transform.localRotation = _RotationLimitHinge.defaultLocalRotation;
        _RotationLimitHinge.Apply();
    }
    */

    // read encoder linear value and convert to rotation
    void ReadInEncoader()
    {
        
    }

    void UpdateRotation()
    {
        // Add any smoothing and accel stuff in here and limits

        // Limit roation to base and extended
        _RotationCurrentAngle = Mathf.Clamp(_RotationCurrentAngle, _RotationBase, _RotationExtended);
        

        // Send out UDP here
        UkiCommunicationsManager.Instance.SendActuatorMessage(this);
    }

    public void OnCalibrationCompleteHandler()
    {

    }
}