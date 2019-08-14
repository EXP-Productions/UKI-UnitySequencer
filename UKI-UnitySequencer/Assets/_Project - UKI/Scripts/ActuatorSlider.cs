using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Associates a UI slider with an actuator
public class ActuatorSlider : MonoBehaviour
{
    public UkiActuatorAssignments _ActuatorAssignment;

    public Actuator _Actuator;

    public TextMeshProUGUI _TextName;

    public Toggle _CollidedToggle;

    public Slider _Slider;

    public Color _NormalCol;
    public Color _CollidedCol;

    private void Start()
    {
        Actuator[] actuators = FindObjectsOfType<Actuator>();
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
            _TextName.text = _ActuatorAssignment.ToString();
        }

        if (_Slider == null)
            print(name);
    }

    private void Update()
    {
        if (_Actuator != null)
        {
            _CollidedToggle.isOn = _Actuator._Collided;
        }
        else
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
