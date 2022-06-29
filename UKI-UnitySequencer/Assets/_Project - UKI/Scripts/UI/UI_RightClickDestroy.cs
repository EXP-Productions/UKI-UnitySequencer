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
            ContextMenuGUI.Instance.Open(new string[]{ "Delete" }, Select );
        }
    }

    void Select(int index)
    {
        Destroy(gameObject);
    }
}