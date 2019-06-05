using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO - Initialize actuator norms to the reported norms - Done
// TODO - place a big red debug sphere at collisions, play collision sound - Both done
// TODO - make it super obvious when estop is triggered
// TODO - display heartbeat in UI
// TODO - Remove send to modbus from UI, make global - done
// TODO - UI should reflect reported positions

/// Basic actuator test that sets the position to move too
public class TestActuator : MonoBehaviour
{
    // State of the limb. Paused, Calibrating, Calibrated or Animating
    public UKIEnums.State _State = UKIEnums.State.Paused;
    // The index of the actuator
    public UkiActuatorAssignments _ActuatorIndex;
    // A copy of the transform that is driven from the read in value
    Transform _ReportedActuatorTransform;

    public CollisionReporter _CollisionReporter;
    [HideInInspector] public CollisionReporter _CollidersToIgnore;

    #region LINEAR EXTENSION
    [Header("LINEAR EXTENSION")]
    // Actuator extension. Linear travel that gets converted into rotational movement   
    // Normalized extension value
    [Range(0, 1)]  public float _NormExtension;
    // Maximum value the encoder can be extended too  
    public float    _MaxEncoderExtension = 40;

    // Speed to send commands at
    [Range(0, 30)]  public int _ExtensionSpeed = 30;
    [Range(30, 60)] public int _BoostExtensionSpeed = 45;
    public bool _BoostSpeedToggled = false;
    float _MaxBoostDuration = 5;
    float _BoostTimer = 0;

    // The current encoder extension is scaled by 10 because modbus is expecting a mm value with a decimal place
    float CurrentEncoderExtension { get { return Mathf.Clamp(Mathf.Clamp01(_NormExtension) * _MaxEncoderExtension * 10, 0, _MaxEncoderExtension * 10); } }

    // The time taken to extend from 0 - 1
    public float _FullExtensionDuration = 10;
    // The time taken to extend from 1 - 0
    public float _FullRetractionDuration = 10;
    #endregion

    #region ROTATION
    [Header("ROTATION MAPPING")]
    // Min and max rotation range
    public Vector3  _RotationAxis = Vector3.right;
    public Vector3  _ForwardAxis = Vector3.up;
    public float _RotationRange = 30;
    float RotationRange { get { return _RotationRange; } }
    Quaternion _InitialRotation;
    #endregion
    
    #region MODBUS
    [Header("MODBUS")]
    public bool _SendToModbus = false;
    [Range(0, 1)] public float _NormReportedExtension;
    // Extension value read in from modbus - Doesn't line up exactly with the set point value we send
    public float _ReportedExtension;
    // The maximum extension reported when extended to the full length of _MaxEncoderExtension
    public float _MaxReportedExtension = 40;    
    // Reporeted speed and accel read in from modbus
    public int ReportedSpeed { private set; get; }
    public int ReportedAcceleration { private set; get; }
    #endregion

    public bool _DEBUG = false;
    public bool _DEBUG_NoModBusSimulationMode = false;

    private void Awake()
    {
        if(_CollisionReporter != null)
            _CollisionReporter.name = "Collision reporter " + _ActuatorIndex.ToString();
    }

    public void Init(Transform reportedActuatorTransform)
    {
        _ReportedActuatorTransform = reportedActuatorTransform;

        // Register the actuator
        UkiStateDB.RegisterActuator(_ActuatorIndex);

        // Start the send position coroutine
        StartCoroutine(SendPosAtRate(15));

        // Set names of the actuators and the reported actuator transforms
        name = "Actuator - " + _ActuatorIndex.ToString();

        if (_ReportedActuatorTransform)
        {
            _ReportedActuatorTransform.name = "REPORTED - " + name;
        }

        _InitialRotation = transform.localRotation;

        if (_CollisionReporter != null)
        {
            _CollisionReporter.OnTriggerReport += OnCollisionReportHandler;
            _CollisionReporter.name = "Collision reporter " + _ActuatorIndex.ToString();
        }

        print("ACTUATOR INIT: " + name + "   MAX EXTENSION: " + _MaxEncoderExtension);
    }

    [ContextMenu("Set rotation axis")]
    public void SetRotationAxis()
    {
        // Set rotation axis from local right axis
        _RotationAxis = transform.TransformDirection(_RotationAxis);
    }

    public void Calibrate()
    {
        _NormExtension = 0f;
        UkiCommunicationsManager.Instance.SendActuatorSetPointCommand(_ActuatorIndex, 0, 30);
    }

    // Update is called once per frame
    void Update()
    {
        if (_ReportedActuatorTransform == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.S))
            SetToReportedExtension();

        // CHECK BOOST
        if (_BoostSpeedToggled)
        {
            _BoostTimer += Time.deltaTime;
            if (_BoostTimer > _MaxBoostDuration)
            {
                _BoostSpeedToggled = false;
                _BoostTimer = 0;
            }
        }
        

        // Set the rotation from normalized extension
        //float rot = _NormExtension.ScaleFrom01(_MinRotationInDegrees, _MaxRotationInDegrees); 
        float rot = _NormExtension.ScaleFrom01(0, RotationRange);
        transform.localRotation = _InitialRotation * Quaternion.AngleAxis(rot, _RotationAxis);

        // Run sim without modbus
        if (_DEBUG_NoModBusSimulationMode)
        {
            if (Mathf.Abs(_ReportedExtension - CurrentEncoderExtension) > 2)
            {
                if (_ReportedExtension < CurrentEncoderExtension)
                {
                    _ReportedExtension += (_MaxReportedExtension / _FullExtensionDuration) * Time.deltaTime;
                }
                else
                {
                    _ReportedExtension -= (_MaxReportedExtension / _FullRetractionDuration) * Time.deltaTime;
                }
            }

            _NormReportedExtension = (float)_ReportedExtension / _MaxReportedExtension;
        }
        else
        {
            // READ IN
            // Get the reported values from mod bus
            _ReportedExtension = (float)UkiStateDB._StateDB[_ActuatorIndex][ModBusRegisters.MB_EXTENSION];
            ReportedSpeed = UkiStateDB._StateDB[_ActuatorIndex][ModBusRegisters.MB_GOTO_SPEED_SETPOINT];
            ReportedAcceleration = UkiStateDB._StateDB[_ActuatorIndex][ModBusRegisters.MB_MOTOR_ACCEL];

            // Update the readin actuator transform
            _NormReportedExtension = (float)_ReportedExtension / _MaxReportedExtension;
            
        }

        // UPDATE REPORTED ACTUATOR
        float reportedRotation = _NormReportedExtension.ScaleFrom01(0, RotationRange);
        _ReportedActuatorTransform.localRotation = _InitialRotation * Quaternion.AngleAxis(reportedRotation, _RotationAxis);

        // UPDATE STATES
        if (_State == UKIEnums.State.Calibrating)
        {
            print(name + "    " + _ReportedExtension);
            if (_ReportedExtension == 0)
                SetState(UKIEnums.State.CalibratedToZero);
        }
    }


    public void SetToReportedExtension()
    {
        _NormExtension = _NormReportedExtension;
    }


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


    [ContextMenu("Set max reported extension")]
    void SetMaxReportedExtension()
    {
        _MaxReportedExtension = _ReportedExtension;
    }


    [ContextMenu("Calibrate Extension Speed")]
    void CalibrateRealWorldExtensionSpeed()
    {
        StartCoroutine(CalibrateRealWorldSpeedRoutine(true));
    }

    [ContextMenu("Calibrate Retraction Speed")]
    void CalibrateRealWorldRetractionsSpeed()
    {
        StartCoroutine(CalibrateRealWorldSpeedRoutine(false));
    }

    IEnumerator CalibrateRealWorldSpeedRoutine(bool extension)
    {
        print("SPEED TEST STARTED: " + name);
        float startTime = Time.time;
        if (extension)
        {
            // From 0 set to full normalized extension
            _NormExtension = 1;
        }
        else
        {
            // From 1 set to 0 normalized extension
            _NormExtension = 0;
        }

        while (_NormReportedExtension != _NormExtension)
        {
            yield return new WaitForEndOfFrame();

            if (extension)
                _FullExtensionDuration = Time.time - startTime;
            else
                _FullRetractionDuration = Time.time - startTime;
        }

        print("SPEED TEST COMPLETED: " + name + " Duration: " + _FullExtensionDuration);
    }

    private void OnCollisionReportHandler(Collider collider)
    {
        if (Time.time < 1)
            return;

        // See if it has collided with another actuated limb
        CollisionReporter actuatorCollider = collider.gameObject.GetComponent<CollisionReporter>();

        // Check if collision is with another collision reporter or other collider like ground plane etc
        if (actuatorCollider != null)
        {
            if(_CollidersToIgnore != actuatorCollider)
            {
                CollidedWithObject(collider.gameObject);
            }
        }
        else
        {
            CollidedWithObject(collider.gameObject);
        }
    }

    public void CollidedWithObject(GameObject go)
    {
        UkiCommunicationsManager.Instance.EStop("COLLISION: " + _CollisionReporter.name + " / " + go.name);

        // Provide visual feedback for collision
        GameObject collisionMarker = Instantiate(SRResources.CollisionMarker.Load());
        collisionMarker.transform.position = go.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

        // Provide audio feedback for collision
        AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = Camera.main.gameObject.AddComponent<AudioSource>();
        }
        audioSource.PlayOneShot(SRResources.collision);

    }

    void SendEncoderExtensionLength()
    {
        UkiCommunicationsManager.Instance.SendActuatorSetPointCommand(_ActuatorIndex, (int)CurrentEncoderExtension, _BoostSpeedToggled ? (int)_BoostExtensionSpeed : (int)_ExtensionSpeed);
    }
    
    void SendCalibrateMessage()
    {
        UkiCommunicationsManager.Instance.SendCalibrationMessage((int)_ActuatorIndex, -(int)_ExtensionSpeed);      
    }
    
    IEnumerator SendPosAtRate(float ratePerSecond)
    {
        float wait = 1f / ratePerSecond;
        float prevPos = 0;
        while (true)
        {
            if (prevPos != CurrentEncoderExtension)
            {
                prevPos = CurrentEncoderExtension;
                SendEncoderExtensionLength();
            }

            yield return new WaitForSeconds(wait);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.TransformPoint(-_RotationAxis * .3f), transform.TransformPoint(_RotationAxis * .3f));
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.TransformPoint(_ForwardAxis * .3f));
    }

    private void OnDrawGizmosSelected()
    {
       
    }
}
