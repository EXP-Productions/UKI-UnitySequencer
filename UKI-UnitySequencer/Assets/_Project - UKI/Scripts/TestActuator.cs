using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Basic actuator test that sets the position to move too
public class TestActuator : MonoBehaviour
{
    public enum State
    {
        Idle,
        Moving,
    }

    public UkiActuatorAssignments _Actuator;
    public State _State = State.Idle;

    // Min and max rotation range
    public float _MinRotationInDegrees = 0;
    public float _MaxRotationInDegrees = 20;

    // Encoder extensions. Linear travel that gets converted into rotational movement
    float _CurrentEncoderExtension = 0;
    public float _MaxEncoderExtension = 40;
    

    [Range(0,1)]
    public float _NormExtension;
 
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            SetEncoderExtension();

        if (Input.GetKeyDown(KeyCode.C))
            Calibrate();

        // Set the rotation from normalized extension
        transform.SetLocalRotX(_NormExtension.ScaleFrom01(_MinRotationInDegrees, _MaxRotationInDegrees));
    }

    [ContextMenu("Send message")]
    void SetEncoderExtension()
    {
        // Set target extension
        _CurrentEncoderExtension = _NormExtension.ScaleFrom01(0, _MaxEncoderExtension);
        UkiCommunicationsManager.Instance.SendActuatorMessage((int)_Actuator, -30);
    }

    [ContextMenu("Calibrate")]
    public void Calibrate()
    {
        UkiCommunicationsManager.Instance.SendCalibrationMessage((int)_Actuator, -30);      
    }
}
