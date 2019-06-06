using System.Collections;
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
    TestActuator[] _AllTestActuators;

    List<PoseData> _AllPoses = new List<PoseData>();
    int _SelectedPose = 0;

    // Start is called before the first frame update
    void Start()
    {
        _AllTestActuators = FindObjectsOfType<TestActuator>();
        LoadAllPoses();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                SavePose(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SavePose(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SavePose(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SavePose(3);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SavePose(4);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                DeletePose();
            }
        }
        else if (Input.GetKey(KeyCode.RightShift))
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
        print("Poses loaded: " + _AllPoses.Count);
    }

    void LoadPose(int index)
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

    void SavePose(int index)
    {
        PoseData newPoseData = new PoseData(UKI_UIManager.Instance._AllActuators);
        _AllPoses.Add(newPoseData);
        JsonSerialisationHelper.Save(Path.Combine(Application.streamingAssetsPath, "UKIPoseData.json"), _AllPoses);
        print("Poses saved: " + _AllPoses.Count);
    }

    void DeletePose()
    {
        if(_AllPoses[_SelectedPose] != null)
            _AllPoses.Remove(_AllPoses[_SelectedPose]);

        print("Poses deleted: " + _AllPoses.Count);
    }
}
