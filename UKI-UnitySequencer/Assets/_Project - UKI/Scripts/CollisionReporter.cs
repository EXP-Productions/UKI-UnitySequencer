using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
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
