using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositioningTool : MonoBehaviour
{
    public Transform _TformToPosition;
    public float _GizmoSize = .01f;

    [ContextMenu("Position")]
    // Update is called once per frame
    void Position()
    {
        Vector3 averagePos = Vector3.zero;

        for (int i = 0; i < transform.childCount; i++)
        {
            averagePos += transform.GetChild(i).position;
        }

        averagePos /= transform.childCount;

        _TformToPosition.position = averagePos;
    }

    [ContextMenu("Orient")]
    // Update is called once per frame
    void Orient()
    {
        _TformToPosition.rotation = Quaternion.LookRotation(transform.GetChild(0).position - transform.GetChild(1).position, Vector3.up);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < transform.childCount; i++)
        {
            Gizmos.DrawSphere(transform.GetChild(i).position, _GizmoSize);
        }
    }
}
