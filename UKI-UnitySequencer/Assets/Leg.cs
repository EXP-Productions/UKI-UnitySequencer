using System.Collections;
using System.Collections.Generic;
using UnityEngine;




// Maps rotations to linear values and back again for leg articulations, so they can be animated or scripted
public class Leg : MonoBehaviour
{
    public Actuator _Hip;
    public Actuator _Knee;
    public Actuator _Ankle;   
   
    
    void Start ()
    {
		
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
