using UnityEngine;
using System;



// Maps rotations to linear values and back again for leg articulations, so they can be animated or scripted
// Renders a target location for the limbs as well as a real world location that is fed back in from the actuator encoders
public class TestRearAndPincers : TestUKILimb
{
    public Actuator _Rear;
    public Actuator _PincerLeft;
    public Actuator _PincerRight;

    void Awake()
    {
        // Make sure it has a leg assignment       
        // Init actuator assignments
        InitActuatorAssignments();

        // Assign actuator aray
        _ActuatorArray = new Actuator[] { _Rear }; //, _PincerLeft, _PincerRight };
	}

    private void Start()
    {
        // Copy leg and assign reported transforms
        GameObject limbCopy = Instantiate(_Rear.gameObject, transform);
        limbCopy.name = "Reported " + _Rear.name;

        // COPY LIMBS SO WE HAVE A PROXY FOR REPORTED REAL WORLD ACTUATOR LENGTHS
        Actuator[] actuators = limbCopy.GetComponentsInChildren<Actuator>();       
        for (int i = 0; i < actuators.Length; i++)
        {
            if(actuators[i]._CollisionReporter!=null)
                Destroy(actuators[i]._CollisionReporter.gameObject);

            Destroy(actuators[i]);
            _ActuatorArray[i].Init(actuators[i].transform);
        }

        // SET MATERIALS ON REPORTED LIMBS
        MeshRenderer[] renderers = limbCopy.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material = SRResources.Limb_Reported;

        // SET MATERIALS ON TARGET LIMBS
        renderers = _Rear.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material = SRResources.Limb_Target;
    }

    [ContextMenu("Assign and rename")]
    protected override void InitActuatorAssignments()
    {
        _Rear._ActuatorIndex = UkiActuatorAssignments.Arse;
        _Rear.name = "Actuator - " + _Rear._ActuatorIndex.ToString();
    }
}
