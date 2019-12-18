﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine.UI.Extensions;

[System.Serializable]
public class PoseData
{
    public string _Name;
    public List<ActuatorData> _ActuatorData = new List<ActuatorData>();

    public PoseData()
    {
    }

    public PoseData(List<Actuator> actuators, string name)
    {
        _Name = name;

        for (int i = 0; i < actuators.Count; i++)
        {
            _ActuatorData.Add(new ActuatorData(actuators[i]));
        }
    }
}



public enum SequencerState
{
    Stopped,
    Playing,
    Paused,
}

public class UKI_PoseManager : MonoBehaviour
{
    #region VARIABLES
    public static UKI_PoseManager Instance;

    // State of sequencer. Playing, paused, stopped
    SequencerState _SequencerState = SequencerState.Paused;

    // List of all actuators
    List<Actuator> AllActuators { get { return UKI_UIManager.Instance._AllActuators; } }

    // List of all poses
    [HideInInspector]
    public List<PoseData> _PoseLibrary = new List<PoseData>();
    // Active pose sequence
    public List<string> _PoseSequence = new List<string>();

    float _SequenceDuration = 0;

    // The current sequence index we are at
    public int _PoseSequenceIndex = 0;

    // Whether or not to mask out the wings
    public bool _MaskWings = false;

    // The range within which an actuator has to be to be considered in that pose
    // i.e. actuator told ot move to 200 and it gets too 170 with an _InPositionRange of 30 it would be considered finished and able to go to the next pose
    public float _InPositionRange = 30;
    
    // How long to hold a pose for
    float _HoldDuration = 0;

    // How long to hold a pose for
    float _NoiseDuration = 0;

    public bool _Debug = false;

    #endregion

    #region UNITY METHODS

    // Start is called before the first frame update
    public void Start()
    {
        Instance = this;
        LoadAllPoses();
    }

    // Update is called once per frame
    void Update()
    {
        if(_SequencerState == SequencerState.Playing)
        {
            if(_PoseSequence.Count == 0)
            {
                SetState(SequencerState.Stopped);
                return;
            }

            // CHECK IF ALL ACTUATORS ARE STOPPED
            float maxTimeToTaget = 0;
            int readyCount = 0;
            for (int i = 0; i < UKI_UIManager.Instance._AllActuators.Count; i++)
            {
                maxTimeToTaget = Mathf.Max(maxTimeToTaget, UKI_UIManager.Instance._AllActuators[i].TimeToTarget());

                if (UKI_UIManager.Instance._AllActuators[i].IsNearTargetPos(SROptions.Current.ActuatorArrivalRange))//UKI_UIManager.Instance._AllActuators[i]._State == UKIEnums.State.Paused || UKI_UIManager.Instance._AllActuators[i]._State == UKIEnums.State.NoiseMovement)
                    readyCount++;
            }

            if (_HoldDuration > 0)
            {
                _HoldDuration -= Time.deltaTime;
                if (_HoldDuration < 0) _HoldDuration = 0;
                print("Holding: " + _HoldDuration);
            }

            if (_Debug)
            {
                print("Max time to target: " + maxTimeToTaget);
                print("Actuators ready count: " + readyCount + "/" + AllActuators.Count);
            }

            // If enough actuators are ready then go to next pose
            if(_HoldDuration == 0 && readyCount == UKI_UIManager.Instance._AllActuators.Count)
            {
                _PoseSequenceIndex++;
                if (_PoseSequenceIndex >= _PoseSequence.Count)
                    _PoseSequenceIndex = 0;

                if(_PoseSequence[_PoseSequenceIndex] == "Hold 10")
                {
                    _HoldDuration = 10;
                }

                SetPoseFromSequence(_PoseSequenceIndex, _MaskWings);

                print("POSE MANAGER - Setting pose index: " + _PoseSequenceIndex + "   ready count: " + readyCount + " / " + UKI_UIManager.Instance._AllActuators.Count);
            }
        }

    }

    #endregion

    // Sets the sequencer state
    public void SetState(SequencerState state)
    {
        Debug.Log("Sequencer state: " + state);

        _SequencerState = state;

        // If stopping then set the sequence index back to 0
        if(_SequencerState == SequencerState.Stopped)
        {
            _PoseSequenceIndex = 0;
            UKI_PoseManager_UI.Instance._StopButton.Select();
        }
        else if (_SequencerState == SequencerState.Paused)
        {
            if(_PoseSequence.Count == 0)
            {
                SetState(SequencerState.Stopped);
                return;
            }

            PauseAllActuators();
            UKI_PoseManager_UI.Instance._PauseButton.Select();
        }
        else if (_SequencerState == SequencerState.Playing)
        {
            PauseAllActuators();
            UKI_PoseManager_UI.Instance._PlayButton.Select();
        }
    }

    void PauseAllActuators()
    {
        for (int i = 0; i < AllActuators.Count; i++)
        {
            AllActuators[i].Stop();
        }

        // Set UI
        UKI_UIManager.Instance.SetActuatorSliders();
    }
     
    public void SetPoseFromSequence(int poseSeqIndex, bool maskWings = false)
    {
        print("Setting pose by sequence index: " + poseSeqIndex);

        _PoseSequenceIndex = poseSeqIndex;
        SetPoseByName(_PoseSequence[_PoseSequenceIndex], _MaskWings);
        UKI_PoseManager_UI.Instance.HighlightSequenceButton();
    }

    void AssessSequenceDuration()
    {
        _SequenceDuration = 0;// _PoseSequence

        float maxDurationBetweenPoses = 0;

        // For the whole sequence
        for (int j = 0; j < _PoseSequence.Count - 1; j++)
        {
            PoseData poseCurrentData = _PoseLibrary.Single(s => s._Name == _PoseSequence[j]);
            PoseData poseNextData = _PoseLibrary.Single(s => s._Name == _PoseSequence[j+1]);

            // for every actuator
            for (int i = 0; i < AllActuators.Count; i++)
            {
                // find the actuator for pose current
                foreach (ActuatorData data in poseCurrentData._ActuatorData)
                {
                    if (AllActuators[i]._ActuatorIndex == data._ActuatorIndex)
                    {


                    }
                }
            }
        }
    }

    public void SetPoseByName(string name, bool maskWings = false)
    {
        print("Setting pose by name: " + name);

        PoseData poseData = _PoseLibrary.Single(s => s._Name == name);

        for (int i = 0; i < AllActuators.Count; i++)
        {
            foreach (ActuatorData data in poseData._ActuatorData)
            {
                if (AllActuators[i]._ActuatorIndex == data._ActuatorIndex)
                {
                    if (maskWings && AllActuators[i]._ActuatorIndex.ToString().Contains("Wing"))
                        continue;
                    else
                        AllActuators[i].NormExtension = data._NormalizedValue;
                }
            }
        }

        // Set UI
        UKI_UIManager.Instance.SetActuatorSliders();
    }


    #region SERIALIZATION

    void LoadAllPoses()
    {
        _PoseLibrary = JsonSerialisationHelper.LoadFromFile<List<PoseData>>(Path.Combine(Application.streamingAssetsPath, "UKIPoseData.json")) as List<PoseData>;

        for (int i = 0; i < _PoseLibrary.Count; i++)
            UKI_PoseManager_UI.Instance.AddPoseButton(_PoseLibrary[i]._Name);

        // Add hold pose
        UKI_PoseManager_UI.Instance.AddPoseButton("Hold 10");

        print(name + " Poses loaded: " + _PoseLibrary.Count);
    }

    public void DeletePose(string poseName)
    {
        PoseData poseData = _PoseLibrary.Single(p => p._Name == poseName);

        if (poseData != null)
        {
            _PoseLibrary.Remove(poseData);    
            JsonSerialisationHelper.Save(Path.Combine(Application.streamingAssetsPath, "UKIPoseData.json"), _PoseLibrary);
            print("Poses deleted: " + _PoseLibrary.Count);
        }
        else
        {
            print("Cannot find pose to remove: " + _PoseLibrary.Count);
        }
    }

    #endregion

    public void PrintAllActuatorRanges()
    {
        int outOfPlace = 0;
        for (int i = 0; i < UKI_UIManager.Instance._AllActuators.Count; i++)
        {
            if (!UKI_UIManager.Instance._AllActuators[i].IsNearTargetPos(SROptions.Current.ActuatorArrivalRange))
            {
                print("ACTUATOR: " + UKI_UIManager.Instance._AllActuators[i] + " Reported extension diff: " + UKI_UIManager.Instance._AllActuators[i]._ReportedExtensionDiff);
                outOfPlace++;
            }
        }

        if (outOfPlace == 0)
        {
            print("All actuators are at set positions.");
        }
    }
}
