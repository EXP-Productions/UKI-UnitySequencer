using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CollisionReporter : MonoBehaviour
{
    public delegate void CollisionReport(Collision collision);
    public event CollisionReport OnCollisionReportEvent;

    public delegate void TriggerReport(Collider collision);
    public event TriggerReport OnTriggerReport;

    public bool _Debug = false;

    void OnCollisionEnter(Collision collision)
    {
        if (_Debug)
            print(name + " Collided with: " + collision.gameObject.name);

        OnCollisionReportEvent?.Invoke(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        if (_Debug)
            print(name + " Collision stay with: " + collision.gameObject.name);

        OnCollisionReportEvent?.Invoke(collision);
    }

    void OnTriggerEnter(Collider other)
    {
        if (_Debug)
            print("Trigger entered: " + name + "   " + other.name);

        OnTriggerReport?.Invoke(other);
    }

    void OnTriggerStay(Collider other)
    {
        if (_Debug)
            print("Trigger stayed: " + name + "   " + other.name);

        OnTriggerReport?.Invoke(other);
    }
}
