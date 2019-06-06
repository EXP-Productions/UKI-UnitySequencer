using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OSCSendRecieveGUI : MonoBehaviour
{
    [SerializeField]
    Image _SendImage;

    [SerializeField]
    Image _RecieveImage;

    [SerializeField]
    Color _SendCol = Color.red;

    [SerializeField]
    Color _RecieveCol = Color.green;

    [SerializeField]
    Color _InactiveCol = Color.gray;

    [SerializeField]
    float _FadeSpeed = 1;
    

    public void SendIndication()
    {
        _SendImage.color = _SendCol;
    }

    public void RecieveIndication()
    {
        _RecieveImage.color = _RecieveCol;
    }

    void Update ()
    {
        _SendImage.color = Color.Lerp(_SendImage.color, _InactiveCol, Time.deltaTime * _FadeSpeed);
        _RecieveImage.color = Color.Lerp(_RecieveImage.color, _InactiveCol, Time.deltaTime * _FadeSpeed);
    }
}
