using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Associates a UI slider with an actuator
public class ActuatorSlider : MonoBehaviour
{
    public TestActuator _Actuator;

    [HideInInspector]
    public Slider _Slider;

    void Start()
    {
        _Slider = gameObject.GetComponent<Slider>();
    }

}
