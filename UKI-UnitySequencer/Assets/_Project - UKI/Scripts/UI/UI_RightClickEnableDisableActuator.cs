using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;


public class UI_RightClickEnableDisableActuator : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent OnRightClickEvent;
    ActuatorSlider selectedSlider;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            selectedSlider = null;
            selectedSlider = eventData.rawPointerPress.gameObject.GetComponentInParent<ActuatorSlider>();

            if (selectedSlider != null)
            {
                if(selectedSlider._Actuator._ActuatorDisabled)
                    ContextMenuGUI.Instance.Open(new string[] { "Enable" }, Enable);
                else
                    ContextMenuGUI.Instance.Open(new string[] { "Disable" }, Disable);
            }

            Debug.Log(eventData.rawPointerPress.gameObject.name, eventData.rawPointerPress.gameObject);
        }
    }

    void Enable(int index)
    {
        selectedSlider._Actuator._ActuatorDisabled = false;
    }

    void Disable(int index)
    {
        selectedSlider._Actuator._ActuatorDisabled = true;
    }
}
