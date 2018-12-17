using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

[System.Serializable]
[RequireComponent(typeof(RotationLimitHinge))]
public class Actuator : MonoBehaviour
{
    public UkiActuatorAssignments _ActuatorIndex = 0;

    public float _MaxLinearTravel = 50;       // Maximum that that linear actuator can travel
    public float _CurrentLinearLength = 0;    // Current length that the linear actuator is at

    public Vector3 _LocalRotationAxis = Vector3.right;
    public float _RotationBase = 0;         // Roation of the joint when it is zeroed/calibrated
    public float _RotationExtended = 30;    // Roation of the joint when it is full extended
    float _RotationCurrentAngle = 0;

    public bool _ReverseGizmoDirection = false;

    RotationLimitHinge _RotationLimitHinge;
    

    // Send message to calibrate actuator to zero
    public void CalibrateToZero()
    {
        // UDP out to calibrate
        _RotationCurrentAngle = 0;
    }

    public void SetFromNorm(float norm)
    {
        //print("Setting from norm: " + norm);

        _RotationCurrentAngle = norm.ScaleFrom01(_RotationBase, _RotationExtended);
        UpdateRotation();
    }

    void UpdateRotation()
    {
        // Add any smoothing and accel stuff in here and limits

        // Limit roation to base and extended
        _RotationCurrentAngle = Mathf.Clamp(_RotationCurrentAngle, _RotationBase, _RotationExtended);

        print(_LocalRotationAxis * _RotationCurrentAngle);

        transform.localRotation = Quaternion.Euler(_LocalRotationAxis * _RotationCurrentAngle);

        // Send out UDP here
    }

    public void OnCalibrationCompleteHandler()
    {

    }

    /*
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector3 direction = _ReverseGizmoDirection ? -_LocalRotationAxis : _LocalRotationAxis;

        // Draw axis line
        Gizmos.DrawLine(transform.position + (transform.TransformDirection(direction) *.1f),
            transform.position + (transform.TransformDirection(direction) * .5f));

        // Draw min angle Line
        Gizmos.DrawLine(transform.position + (transform.TransformDirection(direction) * .1f),
            transform.position + (transform.TransformDirection(direction) * .5f));

        Gizmos.DrawWireSphere(transform.position, .15f);
    }

    void GizmoCircle(Vector3 pos, Vector3 normal, float radius)
    {
        int sides = 20;
        for (int i = 0; i < sides; i++)
        {
            float angle = (float)i / (float)sides;
            angle *= Mathf.PI * 2;

            float x1 = Mathf.Sin(angle) * radius;
            float z1 = Mathf.Cos(angle) * radius;


            float angle2 = (float)i / (float)sides;
            angle2 *= Mathf.PI * 2;

            float x2 = Mathf.Sin(angle2) * radius;
            float z2= Mathf.Cos(angle2) * radius;


            Gizmos.DrawLine(transform.TransformPoint(x1, 0, z1),
                transform.TransformPoint(x2, 0, z2));
        }
    }
    */
}