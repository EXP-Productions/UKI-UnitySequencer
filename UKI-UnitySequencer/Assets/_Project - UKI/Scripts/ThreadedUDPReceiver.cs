using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;


//This Class essentially wraps the native .Net UdpClient class and adds threads to handle the sending and receiving of packets 
//so they don't mess the with main Unity rendering thread. 
public class ThreadedUDPReceiver : MonoBehaviour
{
    public Queue<string> _ReceivedPackets = new Queue<string>();
    public Queue<byte[]> _SendPackets = new Queue<byte[]>();

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
                text = System.Text.Encoding.UTF8.GetString(data);

                lock (_ReceivedPackets)
                    _ReceivedPackets.Enqueue(text);
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
                    payloadBytes = _SendPackets.Dequeue();
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
    public void SendInts(uint[] intVals)
    {
        List<byte> payloadBytesList = new List<byte>();

        for (int i = 0; i < intVals.Length; i++)
            payloadBytesList.AddRange(System.BitConverter.GetBytes(intVals[i]));

        Send(payloadBytesList.ToArray());
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
            _SendPackets.Enqueue(payloadBytes);
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
