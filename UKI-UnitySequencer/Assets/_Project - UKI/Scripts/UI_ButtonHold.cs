using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UI_ButtonHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent _OnDown;
    public UnityEvent _OnUp;

    private bool isDown;
    private float downTime;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_OnDown != null) _OnDown.Invoke();
        this.isDown = true;
        this.downTime = Time.realtimeSinceStartup;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_OnUp != null) _OnUp.Invoke();
        this.isDown = false;
    }

    void Update()
    {
        if (!this.isDown) return;
        if (Time.realtimeSinceStartup - this.downTime > 2f)
        {
            print("Handle Long Tap");
            this.isDown = false;
        }
    }

}