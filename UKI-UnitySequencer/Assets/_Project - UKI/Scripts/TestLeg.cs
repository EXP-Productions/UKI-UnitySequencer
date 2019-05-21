using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLeg : MonoBehaviour
{
    public TestActuator _Hip;
    public TestActuator _Knee;
    public TestActuator _Ankle;

    // Start is called before the first frame update
    void Start()
    {
        _Hip._Actuator = UkiActuatorAssignments.LeftFrontHip;
        _Knee._Actuator = UkiActuatorAssignments.LeftFrontKnee;
        _Ankle._Actuator = UkiActuatorAssignments.LeftFrontAnkle;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
