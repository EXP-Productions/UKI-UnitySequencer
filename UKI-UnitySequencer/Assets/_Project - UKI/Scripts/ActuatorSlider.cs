using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Associates a UI slider with an actuator
public class ActuatorSlider : MonoBehaviour
{
    public UkiActuatorAssignments _ActuatorAssignment;

    public TestActuator _Actuator;

    public Slider _Slider;

    private void Start()
    {
        TestActuator[] actuators = FindObjectsOfType<TestActuator>();
        for (int i = 0; i < actuators.Length; i++)
        {
            if (actuators[i]._ActuatorIndex == _ActuatorAssignment)
                _Actuator = actuators[i];
        }

        if (_Actuator == null)
        {
            print("UI INIT: " + name + " no actuator found");
        }
        else
        {
            _Slider.onValueChanged.AddListener((float f) => _Actuator._NormExtension = f);
        }

        if (_Slider == null)
            print(name);
    }
    
    [ContextMenu("Name")]
    public void Name()
    {
        name = "Actuator Control - " + _ActuatorAssignment.ToString();
    }

    public void SetToActuatorNorm()
    {
        if (_Actuator == null)
            print(name);

        _Slider.normalizedValue = _Actuator._NormExtension;
    }
}
