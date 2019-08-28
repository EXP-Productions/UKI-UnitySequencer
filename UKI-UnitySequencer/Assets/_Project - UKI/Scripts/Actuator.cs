using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO - UI should reflect reported positions
// TODO resurrect timeline manager and see how that works
// TODO hook up IK
// TODO Pose player. Once reaching one pose move to the next pose
// TODO Test out speed boost on actuators
// TODO Draw limb paths 

// TODO Add save name for poses


[System.Serializable]
public class ActuatorData
{
    public float _NormalizedValue;
    public UkiActuatorAssignments _ActuatorIndex;

    public ActuatorData()
    {
    }

    public ActuatorData(Actuator actuator)
    {
        _NormalizedValue = actuator.NormExtension;
        _ActuatorIndex = actuator._ActuatorIndex;
    }
}

public enum ActuatorSide
{
    Left,
    Right,
    Middle,
}

[ExecuteInEditMode]
/// Basic actuator test that sets the position to move too
public class Actuator : MonoBehaviour
{
    #region VARIABLES

    UKI_UIManager _UKIManager;

    // State of the limb. Paused, Calibrating, Calibrated or Animating
    public UKIEnums.State _State = UKIEnums.State.Paused;
    // The index of the actuator
    public UkiActuatorAssignments _ActuatorIndex;
    // A copy of the transform that is driven from the read in value
    Transform _ReportedActuatorTransform;

    public ActuatorSide _Side = ActuatorSide.Left;

    public CollisionReporter _CollisionReporter;
    [HideInInspector] public CollisionReporter _CollidersToIgnore;
    public bool _Collided = false;

    public bool _DEBUG_Waver = false;

    #region LINEAR EXTENSION
    [Header("LINEAR EXTENSION")]
    // Actuator extension. Linear travel that gets converted into rotational movement   
    // Normalized extension value
    [Range(0, 1)]  public float _NormExtension;
    public float NormExtension { get { return _NormExtension; } set { _NormExtension = Mathf.Clamp01(value); } }

    float RotationAngle { get { return Mathf.Lerp(0, RotationRange, NormExtension); } }
    bool AtTargetExtension { get { return _ReportedExtensionDiff < _ReportedTollerance; } }
    // Maximum value the encoder can be extended too  
    public float    _MaxEncoderExtension = 40;

    public float _MovementAnimationDirection = 0;

    // Speed to send commands at
    [Range(0, 30)]  public int _ExtensionSpeed = 30;
    [Range(30, 60)] public int _BoostExtensionSpeed = 45;
    public bool _BoostSpeedToggled = false;
    float _MaxBoostDuration = 5;
    float _BoostTimer = 0;
    public float _OfflineSpeedScaler = 1;

    float _ReportedExtensionDiff;

    // The current encoder extension is scaled by 10 because modbus is expecting a mm value with a decimal place
    float CurrentEncoderExtension { get { return Mathf.Clamp(Mathf.Clamp01(NormExtension) * _MaxEncoderExtension * 10, 0, _MaxEncoderExtension * 10); } }

    // The time taken to extend from 0 - 1
    public float _FullExtensionDuration = 10;
    // The time taken to extend from 1 - 0
    public float _FullRetractionDuration = 10;

    public float MaxExtensionPersecond { get { return 1.0f / _FullExtensionDuration; } }
    public float MaxRetractionDelta { get { return 1.0f / _FullRetractionDuration; } }
    #endregion

    #region ROTATION
    [Header("ROTATION MAPPING")]
    // Min and max rotation range
    public Vector3  _RotationAxis = Vector3.right;
    public Vector3  _ForwardAxis = Vector3.up;
    public float _RotationRange = 30;
    float RotationRange { get { return _RotationRange; } }
    public Quaternion _InitialRotation;
    #endregion
    
    #region MODBUS
    [Header("MODBUS")]
    [Range(0, 1)] public float _NormReportedExtension;
    // Extension value read in from modbus - Doesn't line up exactly with the set point value we send
    public float _ReportedExtension;
    // The maximum extension reported when extended to the full length of _MaxEncoderExtension
    public float _MaxReportedExtension = 40;    
    // Reporeted speed and accel read in from modbus
    public int ReportedSpeed { private set; get; }
    public int ReportedAcceleration { private set; get; }
    #endregion

    // Keeps the previous position so that the actuator only sends a position out if the new position is different
    public float prevPos = 0;

    public bool _DEBUG = false;
    public bool _DEBUG_NoModBusSimulationMode = false;
    public bool _DEBUG_SelfInit = false;
    public bool _Donotsend = false;
    public bool _DEBUG_NoiseMovement = false;

    #endregion

    #region UNITY METHODS

    private void Awake()
    {
        if (_CollisionReporter != null)
            _CollisionReporter.name = "Collision reporter " + _ActuatorIndex.ToString();
    }

    private void Start()
    {
        if(_DEBUG_SelfInit)
        {
            Transform t = GameObject.CreatePrimitive(PrimitiveType.Capsule).transform;
            t.transform.position = Vector3.one * 999;
            Init(t);
        }
    }

    void Update()
    {
        //if (_ReportedActuatorTransform == null)
        //    return;

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

        // ACTUATOR ROTATION
        // Set the rotation from normalized extension       
        transform.localRotation = _InitialRotation * Quaternion.AngleAxis(RotationAngle, _RotationAxis);

        if (_DEBUG)
            print(name + "   " + RotationAngle);

        if (!Application.isPlaying)
            return;

        #region REPORTED EXTENSION

        // Run sim without modbus
        if (_DEBUG_NoModBusSimulationMode)
        {
            if (!UkiCommunicationsManager.Instance._EStopping)
            {
                if (_State == UKIEnums.State.Animating || _State == UKIEnums.State.NoiseMovement)
                {
                    //if (Mathf.Abs(_ReportedExtension - CurrentEncoderExtension) >= _ReportedTollerance)
                    //{
                    // CALCULATE BOOST
                    float boost = 1;
                    if (_BoostSpeedToggled) boost = _BoostExtensionSpeed / _ExtensionSpeed;

                    boost *= _OfflineSpeedScaler;

                    if (_ReportedExtension < CurrentEncoderExtension)
                        _ReportedExtension += (_MaxReportedExtension / _FullExtensionDuration) * Time.deltaTime * boost;
                    else
                        _ReportedExtension -= (_MaxReportedExtension / _FullRetractionDuration) * Time.deltaTime * boost;
                }

                _NormReportedExtension = (float)_ReportedExtension / _MaxReportedExtension;
            }
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

        #endregion

        #region REPORTED ACTUATOR ROTATION

        // UPDATE REPORTED ACTUATOR
        float reportedRotation = _NormReportedExtension.ScaleFrom01(0, RotationRange);
        _ReportedActuatorTransform.localRotation = _InitialRotation * Quaternion.AngleAxis(reportedRotation, _RotationAxis);

        // difference between reported and the set length
        _ReportedExtensionDiff = Mathf.Abs(_ReportedExtension - (NormExtension * _MaxReportedExtension));

        #endregion

        #region  UPDATE STATES

        if (_State == UKIEnums.State.Calibrating)
        {
            print(name + "    " + _ReportedExtension);
            if (_ReportedExtension == 0)
                SetState(UKIEnums.State.CalibratedToZero);
        }
        else if (_State == UKIEnums.State.Paused)
        {
            // IF REPORTED AND TARGET EXTENSION AREN't EQUAL THEN SET TO ANIMATING
            if (_ReportedExtensionDiff >= _ReportedTollerance * 3)
                SetState(UKIEnums.State.Animating);
        }
        else if (_State == UKIEnums.State.Animating)
        {
            // Set movement direction
            if (NormExtension > _NormReportedExtension)
                _MovementAnimationDirection = 1;
            else
                _MovementAnimationDirection = -1;

            // IF REPORTED AND TARGET EXTENSION ARE EQUAL THEN SET TO ANIMATING
            if (AtTargetExtension)
            {
                if(_DEBUG_NoiseMovement)
                    SetState(UKIEnums.State.NoiseMovement);
                else
                    SetState(UKIEnums.State.Paused);
            }           
        }
        else if(_State == UKIEnums.State.NoiseMovement)
        {
            if (AtTargetExtension)
            {
                if (_MovementAnimationDirection > 0)
                {
                    NormExtension = NormExtension - .15f;
                    _MovementAnimationDirection = -1;
                }
                else
                {
                    NormExtension = NormExtension + .15f;
                    _MovementAnimationDirection = 1;
                }
            }
        }

        #endregion
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.TransformPoint(-_RotationAxis * .3f), transform.TransformPoint(_RotationAxis * .3f));
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.TransformPoint(_ForwardAxis * .3f));
    }

    #endregion
    
    public void Init(Transform reportedActuatorTransform)
    {
        _UKIManager = FindObjectOfType<UKI_UIManager>();
        _ReportedActuatorTransform = reportedActuatorTransform;

        // Register the actuator
        UkiStateDB.RegisterActuator(_ActuatorIndex);
        _UKIManager.AddActuator(this);

        // Start the send position coroutine
        StartCoroutine(SendPosAtRate(15));

        // Set names of the actuators and the reported actuator transforms
        name = "Actuator - " + _ActuatorIndex.ToString();

        if (_ActuatorIndex.ToString().Contains("Left"))
            _Side = ActuatorSide.Left;
        else if (_ActuatorIndex.ToString().Contains("Right"))
            _Side = ActuatorSide.Right;
        else
            _Side = ActuatorSide.Middle;

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

    public void Calibrate()
    {
        NormExtension = 0f;
        UkiCommunicationsManager.Instance.SendActuatorSetPointCommand(_ActuatorIndex, 0, 30);
    }

    float _ReportedTollerance = 10;
   
    public void SetToReportedExtension()
    {
        NormExtension = _NormReportedExtension;
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
    
    IEnumerator CalibrateRealWorldSpeedRoutine(bool extension)
    {
        print("SPEED TEST STARTED: " + name);
        float startTime = Time.time;
        if (extension)
        {
            // From 0 set to full normalized extension
            NormExtension = 1;
        }
        else
        {
            // From 1 set to 0 normalized extension
            NormExtension = 0;
        }

        while (_NormReportedExtension != NormExtension)
        {
            yield return new WaitForEndOfFrame();

            if (extension)
                _FullExtensionDuration = Time.time - startTime;
            else
                _FullRetractionDuration = Time.time - startTime;
        }

        print("SPEED TEST COMPLETED: " + name + " Duration: " + _FullExtensionDuration);
    }

    #region COLLISION HANDLING

    private void OnCollisionReportHandler(Collider collider)
    {
        if (Time.time < 1)
            return;

        // See if it has collided with another actuated limb
        CollisionReporter actuatorCollider = collider.gameObject.GetComponent<CollisionReporter>();
        if(actuatorCollider != null)
        {
            if(_CollidersToIgnore != actuatorCollider)
                CollidedWithObject(collider.gameObject);
        }
        else
        {
            if(!_UKIManager._IgnoreCollisions)
                CollidedWithObject(collider.gameObject);
        }
    }

    public void CollidedWithObject(GameObject go)
    {
        if (UkiCommunicationsManager.Instance._EStopping)
            return;

        _Collided = true;

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

    #endregion

    public void ResetEStop()
    {
        // Resets prev pos to set the pos to dirty so it resends
        prevPos = 0;        
        _Collided = false;
    }

    void SendEncoderExtensionLength()
    {
        if (_DEBUG)
            print(CurrentEncoderExtension);

        if (_Side == ActuatorSide.Left && !_UKIManager._LeftEnabledToggle.isOn)
            return;
        else if (_Side == ActuatorSide.Right && !_UKIManager._RightEnabledToggle.isOn)
            return;

        UkiCommunicationsManager.Instance.SendActuatorSetPointCommand(_ActuatorIndex, (int)CurrentEncoderExtension, _BoostSpeedToggled ? (int)_BoostExtensionSpeed : (int)_ExtensionSpeed);
    }
    
    void SendCalibrateMessage()
    {
        UkiCommunicationsManager.Instance.SendCalibrationMessage((int)_ActuatorIndex, -(int)_ExtensionSpeed);      
    }
       
    IEnumerator SendPosAtRate(float ratePerSecond)
    {
        float wait = 1f / ratePerSecond;
        prevPos = 0;
        while (true)
        {
            if (prevPos != CurrentEncoderExtension)
            {
                if (!_Donotsend)
                {
                    prevPos = CurrentEncoderExtension;
                    SendEncoderExtensionLength();
                }
            }

            yield return new WaitForSeconds(wait);
        }
    }

    #region HELPER METHODS

    [ContextMenu("Set rotation axis")]
    public void SetRotationAxis()
    {
        // Set rotation axis from local right axis
        _RotationAxis = transform.TransformDirection(_RotationAxis);
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

    [ContextMenu("Store Initial Rotation")]
    void StoreInitialRotation()
    {
        _InitialRotation = transform.localRotation;
    }

    #endregion
}
