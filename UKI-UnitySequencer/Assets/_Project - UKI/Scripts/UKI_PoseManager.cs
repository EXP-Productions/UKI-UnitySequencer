using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UKI_PoseManager : MonoBehaviour
{
    TestActuator[] _AllTestActuators;

    // Start is called before the first frame update
    void Start()
    {
        _AllTestActuators = FindObjectsOfType<TestActuator>();
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

    void LoadPose(int index)
    {
        print("Loading pose " + index);

        for (int i = 0; i < _AllTestActuators.Length; i++)
        {
            _AllTestActuators[i]._NormExtension = PlayerPrefs.GetFloat("Save_" + index.ToString() + _AllTestActuators[i]._ActuatorIndex.ToString(), 0);
        }
    }

    void SavePose(int index )
    {
        print("Saveing pose " + index);
        for (int i = 0; i < _AllTestActuators.Length; i++)
        {
            // Actuator index
            PlayerPrefs.SetFloat("Save_" + index.ToString() + _AllTestActuators[i]._ActuatorIndex.ToString(), _AllTestActuators[i]._NormExtension);
        }
    }
}
