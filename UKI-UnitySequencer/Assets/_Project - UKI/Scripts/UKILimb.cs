﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

[RequireComponent(typeof(CCDIK))]
[RequireComponent(typeof(EditorIK))]
public class UKILimb : MonoBehaviour
{
    public enum State
    {
        Idle,
        Calibrating,
        CalibratedToZero,
        Animating,
    }

    public State _State = State.Idle;
    protected Actuator[] _ActuatorArray;
    protected CCDIK _IKSolver;
    protected EditorIK _EditorIK;

    private void Start()
    {
        _IKSolver = GetComponent<CCDIK>();
        _EditorIK = GetComponent<EditorIK>();
        _EditorIK.enabled = false;

        // Init the actuator array, setting the parent limb
        for (int i = 0; i < _ActuatorArray.Length; i++)
        {
            _ActuatorArray[i].Init(this);
        }
    }

    private void Update()
    {
        if(_State == State.Calibrating)
        {
            // Check to see if calibration has finished
            int calibratedCount = 0;

            for (int i = 0; i < _ActuatorArray.Length; i++)
            {
                if (_ActuatorArray[i].Calibrated) calibratedCount++;
            }

            if (calibratedCount == _ActuatorArray.Length)
                SetState(State.CalibratedToZero);
        }
    }

    public void SetState(State state)
    {
        _State = state;

        if (_State == State.Animating)
        {
            _IKSolver.enabled = true;
        }
        else if (_State == State.Calibrating)
        {
            // Disable the IK solver so we can set the rotation manually
            _IKSolver.enabled = false;
            
            
            for (int i = 0; i < _ActuatorArray.Length; i++)
            {
                _ActuatorArray[i].CalibrateToZero();
            }
            
        }

        print(name + " State set too: " + _State.ToString());
    }
    
    protected virtual void InitActuatorAssignments()
    {
        print("***   Actuator assignment hasnt been implimented in this limb. " + name);
    }

    public void CalibrateAllToZero()
    {
        SetState(State.Calibrating);
    }

    protected void OnCalibrationCompleteHandler()
    {
        for (int i = 0; i < _ActuatorArray.Length; i++)
        {
            _ActuatorArray[i].OnCalibrationCompleteHandler();
        }
    }
}