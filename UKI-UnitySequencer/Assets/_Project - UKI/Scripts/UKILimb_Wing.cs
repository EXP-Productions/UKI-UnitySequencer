using UnityEngine;
using System;



// Maps rotations to linear values and back again for leg articulations, so they can be animated or scripted
// Renders a target location for the limbs as well as a real world location that is fed back in from the actuator encoders
public class UKILimb_Wing : UKILimb
{
    public UkiWings _WingAssignment; // TODO: change this enum to limb

    public Actuator _Raise;
    public Actuator _Rotate;

    void Awake()
    {
        // Make sure it has a leg assignment       
        // Init actuator assignments
        InitActuatorAssignments();

        // Assign actuator aray
        _ActuatorArray = new Actuator[] { _Raise, _Rotate };
	}

    private void Start()
    {
        // Copy leg and assign reported transforms
        GameObject wingCopy = Instantiate(_Raise.gameObject, transform);
        wingCopy.name = "Reported " + _Raise.name;

        // COPY LIMBS SO WE HAVE A PROXY FOR REPORTED REAL WORLD ACTUATOR LENGTHS
        Actuator[] actuators = wingCopy.GetComponentsInChildren<Actuator>();       
        for (int i = 0; i < actuators.Length; i++)
        {
            if(actuators[i]._CollisionReporter!=null)
                Destroy(actuators[i]._CollisionReporter.gameObject);

            Destroy(actuators[i]);
            _ActuatorArray[i].Init(actuators[i].transform);
        }

        // SET MATERIALS ON REPORTED LIMBS
        MeshRenderer[] renderers = wingCopy.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material = SRResources.Limb_Reported;

        // SET MATERIALS ON TARGET LIMBS
        renderers = _Raise.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material = SRResources.Limb_Target;
    }

    [ContextMenu("Assign and rename")]
    protected override void InitActuatorAssignments()
    {
        //Bit haxy but we're using UkiActuatorAssignments enum to list all the actuator addresses we need to ping messages to,
        //Because these seem to follow a naming convention we can add a bit of convenience code to determine the addresses
        //for hip, knee, ankle addresses based on the particular leg we're looking at.
        string raiseEnum = _Raise.ToString() + "Raise";
        string rotateEnum = _Rotate.ToString() + "Rotate";

        UkiActuatorAssignments[] assignments = (UkiActuatorAssignments[])System.Enum.GetValues(typeof(UkiActuatorAssignments));
        foreach(UkiActuatorAssignments assignment in assignments)
        {
            if (assignment.ToString() == raiseEnum)
            {
                _Raise._ActuatorIndex = assignment;
                _Raise.name = "Actuator - " + assignment.ToString();
            }
            else if (assignment.ToString() == rotateEnum)
            {
                _Rotate._ActuatorIndex = assignment;
                _Rotate.name = "Actuator - " + assignment.ToString();
            }
        }
    }
}
