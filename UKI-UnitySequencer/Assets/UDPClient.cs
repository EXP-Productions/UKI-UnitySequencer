using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class UDPClient : MonoBehaviour
{
    public static UDPClient _Instance;
    Socket _TargetSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    static IPAddress _TargetIP = IPAddress.Parse("192.168.1.57");
    IPEndPoint _TargetEndPoint = new IPEndPoint(_TargetIP, 11000);

    [HideInInspector]
    public string _Message;

    private void Awake()
    {
        _Instance = this;
    }

    void Start ()
    {
		
	}
	
	void Update ()
    {
		
	}

    public void SendUDP()
    {
        byte[] payloadBytes = System.Text.Encoding.ASCII.GetBytes(_Message);
        _TargetSocket.SendTo(payloadBytes, _TargetEndPoint);
        print("Sent " + _Message + " to " + _TargetEndPoint);
    }

}
