using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

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

public class UKI_PoseManager : MonoBehaviour
{
    public static UKI_PoseManager Instance;

    List<Actuator> _AllTestActuators { get { return UKI_UIManager.Instance._AllActuators; } }

    [HideInInspector]
    public List<PoseData> _AllPoses = new List<PoseData>();
    int _SelectedPose = 0;

    public bool _LoopPoses = false;
    public bool _MaskWings = true;

    // Start is called before the first frame update
    public void Start()
    {
        Instance = this;
        LoadAllPoses();
    }

    public float _InPositionRane = 30;

    // Update is called once per frame
    void Update()
    {
        if(_LoopPoses)
        {
            // CHECK IF ALL ACTUATORS ARE STOPPED
            int readyCount = 0;
            for (int i = 0; i < UKI_UIManager.Instance._AllActuators.Count; i++)
            {
                if (UKI_UIManager.Instance._AllActuators[i].IsNearTargetPos(_InPositionRane))//UKI_UIManager.Instance._AllActuators[i]._State == UKIEnums.State.Paused || UKI_UIManager.Instance._AllActuators[i]._State == UKIEnums.State.NoiseMovement)
                    readyCount++;
            }

            //print("Actuators Paused count: " + pausedCount + "/" + _AllTestActuators.Count);

            if(readyCount == UKI_UIManager.Instance._AllActuators.Count - 3)
            {
              

                _SelectedPose++;
                if (_SelectedPose >= _AllPoses.Count)
                    _SelectedPose = 0;

               

                SetPose(_SelectedPose, _MaskWings);

                print("Setting to pose: " + _SelectedPose + "   ready count: " + readyCount + " / " + UKI_UIManager.Instance._AllActuators.Count);
            }
        }

        if (Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetPose(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetPose(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetPose(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SetPose(3);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SetPose(4);
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

    public void SetPose(int index, bool maskWings = false)
    {
        _SelectedPose = index;
        print("Setting pose: " + index);
        for (int i = 0; i < _AllTestActuators.Count; i++)
        {
            foreach (ActuatorData data in _AllPoses[index]._ActuatorData)
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

        UKI_UIManager.Instance.SetActuatorSliders();
    }

    public void SavePose()
    {
        UKI_UIManager.Instance._SavePoseDialog.SetActive(true);
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
