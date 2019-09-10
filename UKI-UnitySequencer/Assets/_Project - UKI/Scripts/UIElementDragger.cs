using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIElementDragger : EventTrigger
{
    private bool dragging;

    Transform _StartParent;

    public void Update()
    {
        if (dragging)
        {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;
        _StartParent = transform.parent;
        transform.SetParent(transform.parent.parent);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
        transform.SetParent(_StartParent);
        transform.SetSiblingIndex(3);
    }
}