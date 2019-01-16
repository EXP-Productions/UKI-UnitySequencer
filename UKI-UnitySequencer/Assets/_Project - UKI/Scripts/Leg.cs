using UnityEngine;
using System;



// Maps rotations to linear values and back again for leg articulations, so they can be animated or scripted
// Renders a target location for the limbs as well as a real world location that is fed back in from the actuator encoders
public class Leg : UKILimb
{
    public UkiLegs _LegAssignment;

    public Actuator _Hip;
    public Actuator _Knee;
    public Actuator _Ankle;

    void Start ()
    {
        if (_LegAssignment != UkiLegs.Undefined)
        {
            InitActuatorAssignments();

            // Assign actuator aray
            _ActuatorArray = new Actuator[] { _Hip, _Knee, _Ankle };
        }
	}

    protected override void InitActuatorAssignments()
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
}
