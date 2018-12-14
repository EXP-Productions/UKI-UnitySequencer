using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;

public class UDPUI : MonoBehaviour
{
    public UDPClient _UDPClient;
    public Button _SendButton;
    public InputField _InputUDP;


    private void Start()
    {
        _UDPClient = UDPClient._Instance;
    }

    void Update()
    {
        _UDPClient._Message = _InputUDP.text;    
    }

}
