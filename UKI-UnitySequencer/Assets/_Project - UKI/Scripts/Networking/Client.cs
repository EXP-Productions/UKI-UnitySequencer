using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    bool _SocketReady;
    TcpClient _Socket;
    NetworkStream _Stream;
    StreamWriter _Writer;
    StreamReader _Reader;

    private void Start()
    {
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        // If already connected return
        if (_SocketReady)
            return;

        // Default values
        string host = "127.0.0.1";
        int port = 6321;

        try
        {
            _Socket = new TcpClient(host, port);
            _Stream = _Socket.GetStream();
            _Writer = new StreamWriter(_Stream);
            _Reader = new StreamReader(_Stream);
            _SocketReady = true;
        }
        catch (Exception e)
        {

            Debug.Log("Socket error: " + e.Message);
        }
    }

    private void Update()
    {
        if(_SocketReady)
        {
            if(_Stream.DataAvailable)
            {
                string data = _Reader.ReadLine();
                if (data != null)
                    OnIncomingData(data);
            }
        }

        //--  DEBUG
        if(Input.GetKeyDown(KeyCode.C))
        {
            WriteBytes(new byte[] { 0, 1, 2, 3, 4, 5, 15, 16, 17, 18, 19, 20 });
            WriteBytes(new byte[] { 15, 16, 17, 18, 19, 20, 99, 98 });
            SendBytes();
        }
    }

    private void OnIncomingData(string data)
    {
        Debug.Log("Server: " + data);
    }

    void Send(string data)
    {
        if (!_SocketReady)
            return;

        _Writer.WriteLine(data);
        _Writer.Flush();
    }

    List<byte> _BytesList = new List<byte>();
    void WriteBytes(byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            _BytesList.Add(data[i]);
        }
    }

    void SendBytes()
    {
        _BytesList.Insert(0, (byte)_BytesList.Count);
        _Stream.Write(_BytesList.ToArray(), 0, _BytesList.Count);
        _Stream.Flush();
        _BytesList.Clear();
    }

    void Send(byte[] data)
    {
        if (!_SocketReady)
            return;

        _Stream.Write(data, 0, data.Length);
        _Stream.Flush();
    }

    void CloseSocket()
    {
        if (!_SocketReady)
            return;

        _Writer.Close();
        _Reader.Close();
        _Socket.Close();
        _SocketReady = false;
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    private void OnDisable()
    {
        CloseSocket();
    }
}
