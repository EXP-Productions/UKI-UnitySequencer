using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;


public class UI_RightClickDestroy : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent OnRightClickEvent;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            //OnRightClickEvent.Invoke();
            Destroy(gameObject);
            Debug.Log("Right Mouse Button Clicked on: " + name);
            UKI_PoseManager_UI.Instance.UpdateSequenceListButtonsAfterWait();
        }
    }

}