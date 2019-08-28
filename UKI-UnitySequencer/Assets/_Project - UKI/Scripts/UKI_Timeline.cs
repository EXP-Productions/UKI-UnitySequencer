using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UKI_Timeline : MonoBehaviour
{
    public UkiActuatorAssignments _Actuator = UkiActuatorAssignments.LeftFrontHip;
    public float _NormExtension = 0;

    public AnimationClip _Clip;

    public float _PlaybackTimer = 0;

    public Animation _Animation;

    public AnimationCurve _Curve;
    public AnimationCurve _Curve2;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _Clip.SampleAnimation(gameObject, _PlaybackTimer);

       // _Clip.SampleAnimation(gameObject, _PlaybackTimer);


        //_Clip.
        //_ActuatorToControl._NormExtension = ;
    }
}
