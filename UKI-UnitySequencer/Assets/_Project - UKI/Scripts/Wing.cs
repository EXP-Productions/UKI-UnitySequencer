using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wing : UKILimb
{
    public Actuator _Lift;
    public Actuator _Rotation;

    void Awake()
    {
        // Assign actuator aray
        _ActuatorArray = new Actuator[] { _Lift, _Rotation };        
    }
}
