using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    #region EVENTS
    public delegate void OnHandleByteData(byte[] bytes);
    public static event OnHandleByteData onHandleByteData;

    public delegate void OnHandleStringData(string s);
    public static event OnHandleStringData onHandleStringData;

    public delegate void OnConnected();
    public static event OnConnected onConnected;

    public delegate void OnDisconnected();
    public static event OnDisconnected onDisconnected;
    #endregion

    public ReadType _ReadType = ReadType.String;
    public int _Port = 9000;
    public string _Host = "127.0.0.1";

    public bool _SocketConnected;
    TcpClient _Socket;
    NetworkStream _Stream;
    StreamWriter _Writer;
    StreamReader _Reader;

    [Header("Debug")]
    public bool _DebugWrite = false;
    public bool _DebugRead = false;

   

    private void Start()
    {
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        // If already connected return
        if (_SocketConnected)
            return;

        try
        {
            _Socket = new TcpClient(_Host, _Port);
            _Stream = _Socket.GetStream();
            _Writer = new StreamWriter(_Stream);
            _Reader = new StreamReader(_Stream);
            _BytesList = new List<byte>();


            onConnected?.Invoke();
            Debug.Log("Socket open too " + _Host + "   " + _Port);

            _SocketConnected = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }
    }

    private void Update()
    {
        if (_Socket == null)
            return;


        _SocketConnected = _Socket.Connected;

        if(_SocketConnected)
        {
            if (_Stream.DataAvailable)
            {
                if (_ReadType == ReadType.String)
                {
                    string data = _Reader.ReadLine();
                    if (data != null)
                        OnIncomingData(data);
                }
                else
                {
                    int count = 6;// _Stream.ReadByte(); // todo set byte read count
                    byte[] byteData = new byte[count];
                    _Stream.Read(byteData, 0, byteData.Length);
                    if (byteData != null)
                        OnIncomingData(byteData);
                }
            }

            if(_ReadType == ReadType.ByteArray)
            {
                SendBytes();
            }
            else
            {
               // Send();
            }
        }

        //--  DEBUG
        if(Input.GetKeyDown(KeyCode.C))
        {
            if (_ReadType == ReadType.ByteArray)
            {
                WriteBytes(new byte[] { 0, 1, 2, 3, 4, 5 });
                WriteBytes(new byte[] { 15, 16, 17, 18, 19, 20});
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
        onHandleStringData?.Invoke(data);

        if (_DebugRead)
        {
            Debug.Log("Server: " + data);
        }
    }

    private void OnIncomingData(byte[] data)
    {
        onHandleByteData?.Invoke(data);

        if (_DebugRead)
        {
           
            string debug = "";
            foreach (byte by in data)
            {
                debug += by.ToString() + ",";
            }

            Debug.Log("Client recieved data: " + debug);
        }
    }
    #endregion

    #region WRITE
    void Send(string data)
    {
        if (!_SocketConnected)
            return;

        _Writer.WriteLine(data);
        _Writer.Flush();
    }

    List<byte> _BytesList = new List<byte>();
    void WriteBytes(byte[] data)
    {
        if (!_SocketConnected)
            return;

        for (int i = 0; i < data.Length; i++)
        {
            _BytesList.Add(data[i]);
        }

    }

    void SendBytes(bool addByteCount = false)
    {
        // Insert bytelist count at start of list
        if(addByteCount)_BytesList.Insert(0, (byte)_BytesList.Count); // Add byte array len

        _Stream.Write(_BytesList.ToArray(), 0, _BytesList.Count);
        _Stream.Flush();
        _BytesList.Clear();
    }

    public void WriteInts(uint[] intVals, bool littleEndian, bool debugPrint = true)
    {
        for (uint i = 0; i < intVals.Length; i++)
        {
            //if (littleEndian)
            //{
            //    _BytesList.AddRange(IntToLittleEndian((short)intVals[i]));
            //}
            //else
            {
                _BytesList.AddRange(System.BitConverter.GetBytes((short)intVals[i]));
            }
        }

        string debug = "";
        foreach (byte by in _BytesList)
        {
            debug += by.ToString() + ",";
        }

        if (_DebugWrite) Debug.Log(Time.timeSinceLevelLoad + "  -  Adding bytes array to stack: " + debug);
    }

    byte[] IntToLittleEndian(short data)
    {
        byte[] b = new byte[2];
        b[0] = (byte)data;
        b[1] = (byte)((data >> 8) & 0xFF);
        return b;
    }
    #endregion

    #region CLEAN UP
    void CloseSocket()
    {
        if (!_SocketConnected)
            return;

        _Writer.Close();
        _Reader.Close();
        _Socket.Close();
        _SocketConnected = false;

        onDisconnected?.Invoke();
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
