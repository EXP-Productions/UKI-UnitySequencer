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

    public float _MaxRotationInDegrees = 20;
    public float _MaxEncoderExtension = 40;

    float _CurrentEncoderExtension = 0;
    float _TargetEncoderExtension = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(_State == State.Moving)
        {
            transform.SetLocalRotX();
        }
    }

    void SetEncoderExtension(float extensionInMM)
    {
        _State = State.Moving;

        // Set target extension
        _TargetEncoderExtension = Mathf.Clamp(extensionInMM, 0, _MaxEncoderExtension);        
    }

    [ContextMenu("Calibrate")]
    public void Calibrate()
    {
        UkiCommunicationsManager.Instance.SendCalibrationMessage((int)_Actuator, -30);      
    }
}
