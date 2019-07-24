using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wing : UKILimb
{
    public Actuator_Old _Lift;
    public Actuator_Old _Rotation;

    void Awake()
    {
        // Assign actuator aray
        _ActuatorArray = new Actuator_Old[] { _Lift, _Rotation };        
    }
}
