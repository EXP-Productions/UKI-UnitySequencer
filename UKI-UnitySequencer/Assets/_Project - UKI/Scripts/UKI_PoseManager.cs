﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

[System.Serializable]
public class PoseData
{
    public List<ActuatorData> _ActuatorData = new List<ActuatorData>();

    public PoseData()
    {
    }

    public PoseData(List<TestActuator> actuators)
    {
        for (int i = 0; i < actuators.Count; i++)
        {
            _ActuatorData.Add(new ActuatorData(actuators[i]));
        }
    }
}

public class UKI_PoseManager : MonoBehaviour
{
    public static UKI_PoseManager Instance;

    TestActuator[] _AllTestActuators;

    List<PoseData> _AllPoses = new List<PoseData>();
    int _SelectedPose = 0;

    bool _LoopPoses = false;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        _AllTestActuators = FindObjectsOfType<TestActuator>();
        LoadAllPoses();
    }

    // Update is called once per frame
    void Update()
    {
        if(_LoopPoses)
        {
            // CHECK IF ALL ACTUATORS ARE STOPPED
        }

        if (Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                LoadPose(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                LoadPose(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                LoadPose(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                LoadPose(3);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                LoadPose(4);
            }
        }
    }

    void LoadAllPoses()
    {
        _AllPoses = JsonSerialisationHelper.LoadFromFile<List<PoseData>>(Path.Combine(Application.streamingAssetsPath, "UKIPoseData.json")) as List<PoseData>;

        for (int i = 0; i < _AllPoses.Count; i++)        
            UKI_UIManager.Instance.AddPoseButton(i);

        print("Poses loaded: " + _AllPoses.Count);
    }

    public void LoadPose(int index)
    {
        _SelectedPose = index;
        print("Loading pose " + index);
        for (int i = 0; i < _AllTestActuators.Length; i++)
        {
            foreach (ActuatorData data in _AllPoses[index]._ActuatorData)
                if (_AllTestActuators[i]._ActuatorIndex == data._ActuatorIndex)
                    _AllTestActuators[i]._NormExtension = data._NormalizedValue;
        }

        UKI_UIManager.Instance.SetActuatorSliders();
    }

    public void SavePose()
    {
        PoseData newPoseData = new PoseData(UKI_UIManager.Instance._AllActuators);
        _AllPoses.Add(newPoseData);
        UKI_UIManager.Instance.AddPoseButton(_AllPoses.Count-1);

        JsonSerialisationHelper.Save(Path.Combine(Application.streamingAssetsPath, "UKIPoseData.json"), _AllPoses);
        print("Poses saved: " + _AllPoses.Count);
    }

    public void DeletePose()
    {
        if(_AllPoses[_SelectedPose] != null)
            _AllPoses.Remove(_AllPoses[_SelectedPose]);

        UKI_UIManager.Instance.RemovePoseButton();

        JsonSerialisationHelper.Save(Path.Combine(Application.streamingAssetsPath, "UKIPoseData.json"), _AllPoses);

        print("Poses deleted: " + _AllPoses.Count);
    }
}
