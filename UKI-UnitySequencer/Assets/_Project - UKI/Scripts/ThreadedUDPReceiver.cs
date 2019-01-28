using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using System;


//This Class essentially wraps the native .Net UdpClient class and adds threads to handle the sending and receiving of packets 
//so they don't mess the with main Unity rendering thread. 
public class ThreadedUDPReceiver : MonoBehaviour
{
    public Queue<byte[]> _ReceivedPackets = new Queue<byte[]>();
    public Stack<byte[]> _SendPackets = new Stack<byte[]>();

    public UdpClient _Client;
    Thread _ReceiveThread;
    Thread _SendThread;

    public bool _UDPReceiverActive = true;

    public int _ReceivePort;
    public string _BroadcastIP;
    public int _BroadcastPort;

    protected void Init(int rcvPort, string bcastIP, int bcastPort)
    {
        _ReceivePort = rcvPort;
        _BroadcastIP = bcastIP;
        _BroadcastPort = bcastPort;
    }

    public virtual void Awake()
    {
        _Client = new UdpClient(_ReceivePort);
        _ReceiveThread = new Thread(
         new ThreadStart(ReceiveData));
        _ReceiveThread.IsBackground = true;
        _ReceiveThread.Start();

        _SendThread = new Thread(
         new ThreadStart(SendData));
        _SendThread.IsBackground = true;
        _SendThread.Start();

    }

    #region " Threaded Methods"
    private void ReceiveData()
    {
        while (_UDPReceiverActive)
        {
            string text = "";
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 11000);
                byte[] data = _Client.Receive(ref anyIP);

                lock (_ReceivedPackets)
                    _ReceivedPackets.Enqueue(data);
            }
            catch (System.Exception ex)
            {
            }
        }
    }

    private void SendData()
    {
        while (_UDPReceiverActive)
        {
            byte[] payloadBytes = null;
            lock (_SendPackets)
            {
                if (_SendPackets.Count > 0)
                {
                    payloadBytes = _SendPackets.Pop();
                }
            }
            if (payloadBytes != null)
            {
                _Client.Send(payloadBytes, payloadBytes.Length, _BroadcastIP, _BroadcastPort);
            }
            else
            {
                Thread.Sleep(10);
            }
        }
    }
    #endregion

    #region "Send Data Methods"
    public void SendInts(uint[] intVals, bool littleEndian)
    {
        List<byte> payloadBytesList = new List<byte>();

        for (uint i = 0; i < intVals.Length; i++)
        {
            if (littleEndian)
            {
                payloadBytesList.AddRange(IntToLittleEndian(intVals[i]));
            }
            else
            {
                payloadBytesList.AddRange(System.BitConverter.GetBytes(intVals[i]));
            }
        }

        string debug = "";
        foreach(byte by in payloadBytesList)
        {
            debug += by.ToString();
        }
        //print("sending bytes array " + debug);
        Send(payloadBytesList.ToArray());
    }

    byte[] IntToLittleEndian(uint data)
    {
        byte[] b = new byte[2];
        b[0] = (byte)data;
        b[1] = (byte)((data >> 8) & 0xFF);
        return b;
    }

    protected void SendString(string payload)
    {
        byte[] payloadBytes = System.Text.Encoding.ASCII.GetBytes(payload);
        Send(payloadBytes);
    }

    protected void Send(byte[] payloadBytes)
    {
        lock (_SendPackets)
        {
            _SendPackets.Push(payloadBytes);
        }
    }
    #endregion

    #region "UDP Client Shutdown"
    private void OnApplicationQuit()
    {
        ShutdownUDP();
    }

    private void OnDestroy()
    {
        ShutdownUDP();
    }

    void ShutdownUDP()
    {
        _UDPReceiverActive = false;
        if (_ReceiveThread != null)
        {
            _ReceiveThread.Interrupt();
            _ReceiveThread = null;
        }
        if (_SendThread != null)
        {
            _SendThread.Interrupt();
            _SendThread = null;
        }
        if (_Client != null)
        {
            _Client.Close();
            _Client = null;
        }
    }

    #endregion
}
