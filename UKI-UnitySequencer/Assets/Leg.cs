using UnityEngine;
using System;



// Maps rotations to linear values and back again for leg articulations, so they can be animated or scripted
public class Leg : MonoBehaviour
{
    public UkiLegs _LegAssignment;

    public Actuator _Hip;
    public Actuator _Knee;
    public Actuator _Ankle;

    public bool _RunDebugTest = false;
    public bool _DebugHip = false;
    public bool _DebugKnee = false;
    public bool _DebugAnkle = false;

    void Start ()
    {
        if (_LegAssignment != UkiLegs.Undefined)
        {
            InitActuatorAssignments();
        }
	}

    void InitActuatorAssignments()
    {
        //Bit haxy but we're using UkiActuatorAssignments enum to list all the actuator addresses we need to ping messages to,
        //Because these seem to follow a naming convention we can add a bit of convenience code to determine the addresses
        //for hip, knee, ankle addresses based on the particular leg we're looking at.
        string hipEnumName = _LegAssignment.ToString() + "Hip";
        string kneeEnumName = _LegAssignment.ToString() + "Knee";
        string ankleEnumName = _LegAssignment.ToString() + "Ankle";

        UkiActuatorAssignments[] assignments = (UkiActuatorAssignments[])System.Enum.GetValues(typeof(UkiActuatorAssignments));
        foreach(UkiActuatorAssignments assignment in assignments)
        {
            if (assignment.ToString() == hipEnumName)
            {
                _Hip._ActuatorIndex = assignment;
            }
            else if (assignment.ToString() == kneeEnumName)
            {
                _Knee._ActuatorIndex = assignment;
            }
            else if (assignment.ToString() == ankleEnumName)
            {
                _Ankle._ActuatorIndex = assignment;
            }
        }
    }

    private void Update()
    {
        if (_RunDebugTest) DebugTest();
    }

    void DebugTest()
    {
        float norm = Mathf.Sin(Time.time).ScaleTo01(-1,1);
        if(_DebugHip) _Hip.SetFromNorm(norm);

      
        norm = Mathf.Sin(Time.time + 1).ScaleTo01(-1, 1);
       if(_DebugKnee) _Knee.SetFromNorm(norm);
        
        norm = Mathf.Sin(Time.time + 2).ScaleTo01(-1, 1);
        if (_DebugAnkle) _Ankle.SetFromNorm(norm);
      
    }

    void CalibrateAllToZero()
    {
        _Hip.CalibrateToZero();
        _Knee.CalibrateToZero();
        _Ankle.CalibrateToZero();
    }

    void OnCalibrationCompleteHandler()
    {
        _Hip.OnCalibrationCompleteHandler();
        _Knee.OnCalibrationCompleteHandler();
        _Ankle.OnCalibrationCompleteHandler();
    }

    void OnDrawGizmos()
    {
        //Gizmos.DrawLine(_Hip.)
    }
}
