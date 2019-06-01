using UnityEngine;
using System;



// Maps rotations to linear values and back again for leg articulations, so they can be animated or scripted
// Renders a target location for the limbs as well as a real world location that is fed back in from the actuator encoders
public class TestLeg : TestUKILimb
{
    public UkiLegs _LegAssignment; // TODO: change this enum to limb

    public TestActuator _Hip;
    public TestActuator _Knee;
    public TestActuator _Ankle;

    void Awake()
    {
        // Make sure it has a leg assignment
        if (_LegAssignment != UkiLegs.Undefined)
        {
            // Init actuator assignments
            InitActuatorAssignments();

            // Assign actuator aray
            _ActuatorArray = new TestActuator[] { _Hip, _Knee, _Ankle };
        }
	}

    private void Start()
    {
        // Copy leg and assign reported transforms
        GameObject legCopy = Instantiate(_Hip.gameObject, transform);
        legCopy.name = "Reported " + _Hip.name;

        TestActuator[] actuators = legCopy.GetComponentsInChildren<TestActuator>();
        print(actuators.Length);
        for (int i = 0; i < actuators.Length; i++)
        {
            Destroy(actuators[i]);
            _ActuatorArray[i].Init(actuators[i].transform);
        }

        MeshRenderer[] renderers = legCopy.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)        
            renderers[i].material = SRResources.ReportedLimbMat;
        

        renderers = _Hip.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)        
            renderers[i].material = SRResources.Target_Limb_Mat;
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
