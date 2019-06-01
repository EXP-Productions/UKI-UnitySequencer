using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Collider that tells an actuator to stop if it is going to collide with an object 
public class CollisionReporter : MonoBehaviour
{
    public delegate void CollisionReport(Collision collision);
    public event CollisionReport OnCollisionReportEvent;
    
    void OnCollisionEnter(Collision collision)
    {
        if(OnCollisionReportEvent != null)
            OnCollisionReportEvent(collision);
    }
}
