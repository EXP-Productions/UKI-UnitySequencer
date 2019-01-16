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
    enum State
    {
        Idle,
        Calibrating,
        CalibratedToZero,
        Animating,
    }

    State _State = State.Idle;

    public UkiActuatorAssignments _ActuatorIndex = 0;

    public float _MaxLinearTravel = 50;       // Maximum that that linear actuator can travel
    public float _CurrentLinearLength = 0;    // Current length that the linear actuator is at
    public float _TargetLinearLength = 0;    // Target length that the linear actuator is aiming for

    public Vector3 _LocalRotationAxis = Vector3.right;
    public float _RotationBase = 0;         // Roation of the joint when it is zeroed/calibrated
    public float _RotationExtended = 30;    // Roation of the joint when it is full extended

    float _RotationTargetAngle = 0;     // Target angle for roation
    float _RotationCurrentAngle = 0;    // Currecnt angle for rotation
    
    RotationLimitHinge _RotationLimitHinge; // Limit hinge used in the IK. Used to set the min and max angles

    private void Start()
    {
        _RotationLimitHinge = GetComponent<RotationLimitHinge>();
    }

    void SetState(State state)
    {
        _State = state;
    }

    // Send message to calibrate actuator to zero
    public void CalibrateToZero()
    {
        transform.localRotation = _RotationLimitHinge.defaultLocalRotation;
    }

    public void SetFromNorm(float norm)
    {
        //print("Setting from norm: " + norm);

        _RotationCurrentAngle = norm.ScaleFrom01(_RotationBase, _RotationExtended);
        UpdateRotation();
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
        

        // Send out UDP here
        UkiCommunicationsManager.Instance.SendActuatorMessage(this);
    }

    public void OnCalibrationCompleteHandler()
    {

    }
}