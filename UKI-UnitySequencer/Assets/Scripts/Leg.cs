using System.Collections;
using System.Collections.Generic;
using UnityEngine;




// Maps rotations to linear values and back again for leg articulations, so they can be animated or scripted
public class Leg : MonoBehaviour
{
    public Actuator _Hip;
    public Actuator _Knee;
    public Actuator _Ankle;

    public bool _RunDebugTest = false;
    public bool _DebugHip = false;
    public bool _DebugKnee = false;
    public bool _DebugAnkle = false;

    void Start ()
    {
		
	}

    private void Update()
    {
        if (_RunDebugTest) DebugTest();
    }

    void DebugTest()
    {
        float norm = Mathf.Sin(Time.time).ScaleTo01(-1,1);
        if(_DebugHip) _Hip.SetFromNorm(norm);

      
        norm = Mathf.Sin(Time.time + 1).ScaleTo01(-1, 1);
       if(_DebugKnee) _Knee.SetFromNorm(norm);
        
        norm = Mathf.Sin(Time.time + 2).ScaleTo01(-1, 1);
        if (_DebugAnkle) _Ankle.SetFromNorm(norm);
      
    }

    void CalibrateAllToZero()
    {
        _Hip.CalibrateToZero();
        _Knee.CalibrateToZero();
        _Ankle.CalibrateToZero();
    }

    void OnCalibrationCompleteHandler()
    {
        _Hip.OnCalibrationCompleteHandler();
        _Knee.OnCalibrationCompleteHandler();
        _Ankle.OnCalibrationCompleteHandler();
    }

    void OnDrawGizmos()
    {
        //Gizmos.DrawLine(_Hip.)
    }
}
