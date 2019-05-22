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

    [Range(0,30)]
    public float _Speed = 15;

    // Min and max rotation range
    public float _MinRotationInDegrees = 0;
    public float _MaxRotationInDegrees = 20;

    // Encoder extensions. Linear travel that gets converted into rotational movement
    float _CurrentEncoderExtension { get { return Mathf.Clamp(_NormExtension * _MaxEncoderExtension * 10, 0, _MaxEncoderExtension * 10); } }
    public float _MaxEncoderExtension = 40;

    public bool _SendPosAtUpdateRate = false;

    public bool _SinTest = false;
    public float _SinFreq = 1;

    public int _ReportedExtension;
    public int _ReportedSpeed;


    [Range(0,1)]
    public float _NormExtension;

    private void Start()
    {
        StartCoroutine(SendPosAtRate(15));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            SetEncoderExtension();

        if (Input.GetKeyDown(KeyCode.C))
            Calibrate();

        if (_SinTest)
        {
            _NormExtension = Mathf.Sin(Time.time * Mathf.PI * 2 * _SinFreq).ScaleTo01(-1, 1);
            print(_NormExtension + "     " + _CurrentEncoderExtension);
        }

        _ReportedExtension = UkiStateDB._StateDB[_Actuator][ModBusRegisters.MB_EXTENSION];
        _ReportedSpeed = UkiStateDB._StateDB[_Actuator][ModBusRegisters.MB_MOTOR_SPEED];

        // Set the rotation from normalized extension
        transform.SetLocalRotX(_NormExtension.ScaleFrom01(_MinRotationInDegrees, _MaxRotationInDegrees));
    }

    IEnumerator SendPosAtRate(float ratePerSecond)
    {
        float wait = 1f / ratePerSecond;
        while (true)
        {           
            if (_SendPosAtUpdateRate)
            {               
                SetEncoderExtension();
            }

            yield return new WaitForSeconds(wait);
        }
    }

    [ContextMenu("Send message")]
    void SetEncoderExtension()
    {
        UkiCommunicationsManager.Instance.SendActuatorSetPointCommand(_Actuator, (int)_CurrentEncoderExtension, (int)_Speed);
    }

    [ContextMenu("Calibrate")]
    public void Calibrate()
    {
        UkiCommunicationsManager.Instance.SendCalibrationMessage((int)_Actuator, -30);      
    }
}
