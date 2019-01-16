using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UKILimb : MonoBehaviour
{
    public enum State
    {
        Idle,
        Calibrating,
        CalibratedToZero,
        Animating,
    }

    protected State _State = State.Idle;
    protected Actuator[] _ActuatorArray;

    protected void CalibrateAllToZero()
    {
        for (int i = 0; i < _ActuatorArray.Length; i++)
        {
            _ActuatorArray[i].CalibrateToZero();
        }
    }

    protected void OnCalibrationCompleteHandler()
    {
        for (int i = 0; i < _ActuatorArray.Length; i++)
        {
            _ActuatorArray[i].OnCalibrationCompleteHandler();
        }
    }

    protected virtual void InitActuatorAssignments()
    {
        print("***   Actuator assignment hasnt been implimented in this limb. " + name);
    }
}
