using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Associates a UI slider with an actuator
public class ActuatorSlider : MonoBehaviour
{
    public TestActuator _Actuator;

    public Slider _Slider;

    public Toggle _SendToModbus;

    private void Update()
    {
        _Actuator._SendToModbus = _SendToModbus.isOn;
        print(_SendToModbus.isOn);
    }

}
