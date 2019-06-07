using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ClickDrag : MonoBehaviour, IPointerDownHandler
{
    bool _Clicked = false;

    public Transform _CamPivot;
    public float _XSpeed = .1f;
    public float _YSpeed = .1f;
    public float _ZoomSpeed = .1f;

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        _Clicked = true;
        print(name);
    }

    void Update()
    {
        if(_Clicked)
        {
            _CamPivot.Rotate(Vector3.up * Input.GetAxis("Mouse X") * _XSpeed);

            if (Input.GetMouseButtonUp(0))
                _Clicked = false;
        }

        Camera.main.transform.SetLocalZ(Camera.main.transform.localPosition.z + Input.GetAxis("Mouse ScrollWheel") * _ZoomSpeed);
    }
}
