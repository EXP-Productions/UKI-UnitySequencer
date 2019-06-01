﻿using System.Collections;
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

    public Toggle _SendToModbus;

    private void Update()
    {
        _Actuator._SendToModbus = _SendToModbus.isOn;
    }

    [ContextMenu("Name")]
    public void Name()
    {
        name = "Actuator Control - " + _ActuatorAssignment.ToString();
    }
}
