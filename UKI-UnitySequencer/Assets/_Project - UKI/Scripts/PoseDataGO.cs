using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PoseDataGO : MonoBehaviour
{
    public PoseData _PoseData;
    public bool _Preview = true;
    bool _Init = false;

    public Actuator[] _AllActuators;

    Dictionary<UkiActuatorAssignments, Actuator> _ActuatorDict;

    void Init()
    {
        _ActuatorDict = new Dictionary<UkiActuatorAssignments, Actuator>();
        _AllActuators = FindObjectsOfType<Actuator>();

        for (int i = 0; i < _AllActuators.Length; i++)
        {
            _ActuatorDict.Add(_AllActuators[i]._ActuatorIndex, _AllActuators[i]);
        }

        _Init = true;
    }

    public void Update()
    {
        if (!_Init)
            Init();

        if (_Preview)// && !Application.isPlaying)
        {
            for (int i = 0; i < _PoseData._ActuatorDataList.Count; i++)
            {
                print(_PoseData._ActuatorDataList[i]._ActuatorIndex.ToString());
                print(_ActuatorDict[_PoseData._ActuatorDataList[i]._ActuatorIndex].name);
                _ActuatorDict[_PoseData._ActuatorDataList[i]._ActuatorIndex].TargetNormExtension = _PoseData._ActuatorDataList[i]._NormalizedValue;
            }
        }
    }
}
