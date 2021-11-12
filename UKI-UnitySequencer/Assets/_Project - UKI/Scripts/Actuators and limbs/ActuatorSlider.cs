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
    public Color _AnimatingCol = Color.green;

    ColorBlock _ColBlock;

    private void Start()
    {
        Actuator[] actuators = FindObjectsOfType<Actuator>();
        for (int i = 0; i < actuators.Length; i++)
        {
            if (actuators[i]._ActuatorIndex == _ActuatorAssignment)
                _Actuator = actuators[i];
        }

        _ColBlock = new ColorBlock();
        _ColBlock.normalColor = Color.gray;
        _ColBlock.highlightedColor = Color.gray * 1.3f;
        _ColBlock.pressedColor = Color.gray * .65f;

        if (_Actuator == null)
        {
            print("UI INIT: " + name + " no actuator found");
        }
        else
        {
            _Slider.onValueChanged.AddListener((float f) => _Actuator.TargetNormExtension = f);
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


        switch (_Actuator._State)
        {
            case UKIEnums.State.Animating:
                _ColBlock.normalColor = Color.yellow;
                break;
            case UKIEnums.State.Paused:
                _ColBlock.normalColor = Color.gray;
                break;
            case UKIEnums.State.NoiseMovement:
                _ColBlock.normalColor = Color.blue;
                break;
        }

        _Slider.colors = _ColBlock;
    }

    [ContextMenu("Name")]
    public void Name()
    {
        name = "Actuator Control - " + _ActuatorAssignment.ToString();
    }

    public void SetToActuatorNorm(bool setWithoutNotify)
    {
        if (_Actuator == null)
            print(name);


        if (setWithoutNotify)
            _Slider.SetValueWithoutNotify(_Actuator.TargetNormExtension);
        else
            _Slider.normalizedValue = _Actuator.TargetNormExtension;
    }
}
