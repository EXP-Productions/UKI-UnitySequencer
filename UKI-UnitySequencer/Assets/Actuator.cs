using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Actuator : MonoBehaviour
{
    public int _ActuatorIndex = 0;

    public float _MaxLinearTravel = 50;       // Maximum that that linear actuator can travel
    public float _CurrentLinearLength = 0;    // Current length that the linear actuator is at

    public Vector3 _LocalRotationAxis = Vector3.right;
    public float _RotationBase = 0;         // Roation of the joint when it is zeroed/calibrated
    public float _RotationExtended = 30;    // Roation of the joint when it is full extended

    // Send message to calibrate actuator to zero
    public void CalibrateToZero()
    {
        // UDP out to calibrate
    }

    public void OnCalibrationCompleteHandler()
    {

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + (transform.TransformDirection(_LocalRotationAxis) *.1f),
            transform.position + (transform.TransformDirection(_LocalRotationAxis) * .5f));

        for (int i = 0; i < 12; i++)
        {
            float angle = 0;
            //Gizmos.DrawLine(transform.position);
        }
    }

}