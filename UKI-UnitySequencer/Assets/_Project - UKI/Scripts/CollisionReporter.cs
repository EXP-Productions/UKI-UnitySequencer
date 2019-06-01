using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CollisionReporter : MonoBehaviour
{
    public delegate void CollisionReport(Collision collision);
    public event CollisionReport OnCollisionReportEvent;

    public bool _Debug = false;

    void OnCollisionEnter(Collision collision)
    {
        if (_Debug)
            print(name + " Collided with: " + collision.gameObject.name);

        OnCollisionReportEvent?.Invoke(collision);
    }
}
