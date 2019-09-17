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
    public static UKI_PoseManager Instance;

    List<Actuator> _AllTestActuators { get { return UKI_UIManager.Instance._AllActuators; } }

    [HideInInspector]
    public List<PoseData> _PoseLibrary = new List<PoseData>();
    public List<string> _PoseSequence = new List<string>();
    public int _PoseSequenceIndex = 0;

    SequencerState _SequencerState = SequencerState.Paused;

    public bool _MaskWings = false;

    public float _InPositionRane = 30;

    public bool _Debug = false;
   
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
            int readyCount = 0;
            for (int i = 0; i < UKI_UIManager.Instance._AllActuators.Count; i++)
            {
                if (UKI_UIManager.Instance._AllActuators[i].IsNearTargetPos(_InPositionRane))//UKI_UIManager.Instance._AllActuators[i]._State == UKIEnums.State.Paused || UKI_UIManager.Instance._AllActuators[i]._State == UKIEnums.State.NoiseMovement)
                    readyCount++;
            }

            if(_Debug)
                print("Actuators ready count: " + readyCount + "/" + _AllTestActuators.Count);

            if(readyCount == UKI_UIManager.Instance._AllActuators.Count)
            {
                _PoseSequenceIndex++;
                if (_PoseSequenceIndex >= _PoseSequence.Count)
                    _PoseSequenceIndex = 0;

                SetPoseFromSequence(_PoseSequenceIndex, _MaskWings);

                print("POSE MANAGER - Setting pose index: " + _PoseSequenceIndex + "   ready count: " + readyCount + " / " + UKI_UIManager.Instance._AllActuators.Count);
            }
        }
    }

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
        for (int i = 0; i < _AllTestActuators.Count; i++)
        {
            _AllTestActuators[i].Stop();
        }

        // Set UI
        UKI_UIManager.Instance.SetActuatorSliders();
    }

    void LoadAllPoses()
    {
        _PoseLibrary = JsonSerialisationHelper.LoadFromFile<List<PoseData>>(Path.Combine(Application.streamingAssetsPath, "UKIPoseData.json")) as List<PoseData>;

        for (int i = 0; i < _PoseLibrary.Count; i++)      
           UKI_PoseManager_UI.Instance.AddPoseButton(_PoseLibrary[i]._Name);

        print(name + " Poses loaded: " + _PoseLibrary.Count);
    }

    public void SetPoseByName(string name, bool maskWings = false)
    {
        print("Setting pose by name: " + name);

        PoseData poseData = _PoseLibrary.Single(s => s._Name == name);

        for (int i = 0; i < _AllTestActuators.Count; i++)
        {
            foreach (ActuatorData data in poseData._ActuatorData)
            {
                if (_AllTestActuators[i]._ActuatorIndex == data._ActuatorIndex)
                {
                    if (maskWings && _AllTestActuators[i]._ActuatorIndex.ToString().Contains("Wing"))
                        continue;
                    else
                        _AllTestActuators[i].NormExtension = data._NormalizedValue;
                }
            }
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
}
