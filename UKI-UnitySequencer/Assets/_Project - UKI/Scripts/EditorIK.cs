using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

[ExecuteInEditMode]
public class EditorIK : MonoBehaviour
{
    private IK ik;

    void Start()
    {
        ik = GetComponent<IK>();
        ik.GetIKSolver().Initiate(ik.transform);
    }

    void Update()
    {
        if (ik == null) return;

        if (ik.fixTransforms) ik.GetIKSolver().FixTransforms();

        // Apply animation here if you want

        ik.GetIKSolver().Update();
    }
}