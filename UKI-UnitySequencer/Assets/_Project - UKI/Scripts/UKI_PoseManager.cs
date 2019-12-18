using System.Collections;
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
    public List<ActuatorData> _ActuatorDataList = new List<ActuatorData>();

    public PoseData()
    {
    }

    public PoseData(Dictionary<UkiActuatorAssignments, Actuator> actuators, string name)
    {
        _Name = name;

        foreach (KeyValuePair<UkiActuatorAssignments, Actuator> actuator in UKI_UIManager.Instance._AllActuators)
        {
            _ActuatorDataList.Add(new ActuatorData(actuator.Value));
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
      


        if (_SequencerState == SequencerState.Playing)
        {
            if(_PoseSequence.Count == 0)
            {
                SetState(SequencerState.Stopped);
                return;
            }

            // CHECK IF ALL ACTUATORS ARE STOPPED
            float maxTimeToTaget = 0;
            int readyCount = 0;

            foreach (KeyValuePair<UkiActuatorAssignments, Actuator> actuator in UKI_UIManager.Instance._AllActuators)
            {
                maxTimeToTaget = Mathf.Max(maxTimeToTaget, actuator.Value.TimeToTarget());

                if (actuator.Value.IsNearTargetPos(SROptions.Current.ActuatorArrivalRange))
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
                print("Actuators ready count: " + readyCount + "/" + UKI_UIManager.Instance._AllActuators.Count);
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
        else
        {
            if (Input.GetKeyDown(KeyCode.P))
                AssessSequenceDuration();
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
        foreach (KeyValuePair<UkiActuatorAssignments, Actuator> actuator in UKI_UIManager.Instance._AllActuators)
            actuator.Value.Stop();        

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

    public void ScrubSequence(float norm)
    {
        LerpBetweenPoses(GetPoseData(0), GetPoseData(1), norm);
    }

    void LerpBetweenPoses(PoseData fromData, PoseData toData, float normLerp)
    {
        print("LerpBetweenPoses : " + fromData._Name + "    " + toData._Name + "   Norm: " + normLerp);
        
        foreach (KeyValuePair<UkiActuatorAssignments, Actuator> actuator in UKI_UIManager.Instance._AllActuators)
        {
            ActuatorData actuatorDataFrom = fromData._ActuatorDataList.Find(e => e._ActuatorIndex == actuator.Key);
            ActuatorData actuatorDataTo = toData._ActuatorDataList.Find(e => e._ActuatorIndex == actuator.Key);

            float normExtension = Mathf.Lerp(actuatorDataFrom._NormalizedValue, actuatorDataTo._NormalizedValue, normLerp);

            UKI_UIManager.Instance._AllActuators[actuator.Key]._TargetNormExtension = normExtension;

            print("Lerping: " + actuator.Key.ToString());
        }
    }

  

    // Doesn't currently work with holds
    void AssessSequenceDuration()
    {        
        _SequenceDuration = 0;
        float totalDuration = 0;

        // For the whole sequence
        for (int j = 0; j < _PoseSequence.Count - 1; j++)
        {
            // Test the time from the current pose to the next pose
            PoseData poseCurrentData = GetPoseData(j);
            PoseData poseNextData = GetPoseData(j+1);

            /*
            if (poseNextData._Name.Contains("Hold"))
            {
                totalDuration += 10;
            }
            else
            {
            */
            float maxDurationBetweenPoses = 0;

                for (int i = 0; i < poseCurrentData._ActuatorDataList.Count; i++)
                {
                    Actuator actuator = UKI_UIManager.Instance._AllActuators[poseCurrentData._ActuatorDataList[i]._ActuatorIndex];
                    float timeBetweenPoses = actuator.TimeToTargetFrom(poseCurrentData._ActuatorDataList[i]._NormalizedValue, poseNextData._ActuatorDataList[i]._NormalizedValue);

                    if (timeBetweenPoses > maxDurationBetweenPoses)
                        maxDurationBetweenPoses = timeBetweenPoses;
                }

                totalDuration += maxDurationBetweenPoses;
            //}
        }

        print("Total sequence duration: " + totalDuration);
    }

    public void SetPoseByName(string name, bool maskWings = false)
    {
        print("Setting pose by name: " + name);

        PoseData poseData = _PoseLibrary.Single(s => s._Name == name);

        // for each actuator in pose data
        for (int i = 0; i < poseData._ActuatorDataList.Count; i++)
        {
            Actuator actuator = UKI_UIManager.Instance._AllActuators[poseData._ActuatorDataList[i]._ActuatorIndex];

            // set actuator target norm extension unless wing mask is active
            if (maskWings && poseData._ActuatorDataList[i]._ActuatorIndex.ToString().Contains("Wing"))
                continue;
            else
                UKI_UIManager.Instance._AllActuators[poseData._ActuatorDataList[i]._ActuatorIndex].TargetNormExtension = poseData._ActuatorDataList[i]._NormalizedValue;
        }

        // Set UI
        UKI_UIManager.Instance.SetActuatorSliders();
    }

    PoseData GetPoseData(int poseIndex)
    {
        print("Trying to get pose: " + poseIndex + " from pose sequence list of count: " + _PoseSequence.Count);
        return _PoseLibrary.Single(s => s._Name == _PoseSequence[poseIndex]);
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

        foreach (KeyValuePair<UkiActuatorAssignments, Actuator> actuator in UKI_UIManager.Instance._AllActuators)
        {
            if(actuator.Value.IsNearTargetPos(SROptions.Current.ActuatorArrivalRange))
            {
                print("ACTUATOR: " + actuator.Value + " Reported extension diff: " + actuator.Value._ReportedExtensionDiff);
                outOfPlace++;
            }
        }

        if (outOfPlace == 0)
        {
            print("All actuators are at set positions.");
        }
    }
}
