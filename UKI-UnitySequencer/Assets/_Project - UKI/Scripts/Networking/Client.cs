using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public ReadType _ReadType = ReadType.String;
    public int _Port = 8001;
    public string _Host = "127.0.0.1";

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

        try
        {
            _Socket = new TcpClient(_Host, _Port);
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
                if (_ReadType == ReadType.String)
                {
                    string data = _Reader.ReadLine();
                    if (data != null)
                        OnIncomingData(data);
                }
                else
                {
                    int count = _Stream.ReadByte();
                    byte[] byteData = new byte[count];
                    _Stream.Read(byteData, 0, byteData.Length);
                    if (byteData != null)
                        OnIncomingData(byteData);
                }
            }
        }

        //--  DEBUG
        if(Input.GetKeyDown(KeyCode.C))
        {
            if (_ReadType == ReadType.ByteArray)
            {
                WriteBytes(new byte[] { 0, 1, 2, 3, 4, 5, 15, 16, 17, 18, 19, 20 });
                WriteBytes(new byte[] { 15, 16, 17, 18, 19, 20, 99, 98 });
                SendBytes();
            }
            else
            {
                Send("Test string");
            }
        }
    }

    #region READ
    private void OnIncomingData(string data)
    {
        Debug.Log("Server: " + data);
    }

    private void OnIncomingData(byte[] data)
    {
        Debug.Log("Recieved data of length : " + data.Length);
        for (int i = 0; i < data.Length; i++)
        {
            Debug.Log(i + " - " + data[i]);
        }
    }
    #endregion

    #region WRITE
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
        // Insert bytelist count at start of list
        _BytesList.Insert(0, (byte)_BytesList.Count);

        _Stream.Write(_BytesList.ToArray(), 0, _BytesList.Count);
        _Stream.Flush();
        _BytesList.Clear();
    }
    #endregion

    #region CLEAN UP
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
    #endregion
}
