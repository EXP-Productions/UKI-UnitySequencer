using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class TestUKILimb : MonoBehaviour
{
    // State of the limb. Paused, Calibrating, Calibrated or Animating
    public UKIEnums.State _State = UKIEnums.State.Paused;
    // Array of actuators in the limb
    protected Actuator[] _ActuatorArray;

    // Controls all linear norms in chain
    public bool _ControlAllLinear = false;
    [Range(0, 1)]
    public float _NormExtension = 0;


    private void Update()
    {
        // Handle inputs
        // Send extensions manually
        if (Input.GetKeyDown(KeyCode.C))
        {
            SetState(UKIEnums.State.Calibrating);
        }

        // If claibrating check all the actuator joints to see if they are calibrated
        if (_State == UKIEnums.State.Calibrating)
        {
            int calibratedCount = 0;
            for (int i = 0; i < _ActuatorArray.Length; i++)
                if (_ActuatorArray[i].IsCalibrated())
                    calibratedCount++;

            // If they are all calibrated then set the state too Calibrated to zero
            if (calibratedCount == _ActuatorArray.Length)
                SetState(UKIEnums.State.CalibratedToZero);
        }

        // Control all linear movements at once
        if(_ControlAllLinear)
        {
            for (int i = 0; i < _ActuatorArray.Length; i++)
            {
                _ActuatorArray[i].NormExtension = _NormExtension;
            }
        }
    }

    public void SetState(UKIEnums.State state)
    {
        _State = state;

        if (_State == UKIEnums.State.Animating)
        {
           // _IKSolver.enabled = true;
        }
        

        print(name + " State set too: " + _State.ToString());
    }
    
    protected virtual void InitActuatorAssignments()
    {
        print("***   Actuator assignment hasnt been implimented in this limb. " + name);
    }

    public void CalibrateAllToZero()
    {
        SetState(UKIEnums.State.Calibrating);
    }
}
