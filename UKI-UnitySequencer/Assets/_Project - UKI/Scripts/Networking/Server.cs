using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public enum ReadType
{
    String,
    ByteArray
}

public class Server : MonoBehaviour
{
    List<ServerClient> _Clients = new List<ServerClient>();
    List<ServerClient> _DisconnectList = new List<ServerClient>();

    public ReadType _ReadType = ReadType.String;
    public string _LocalIP;
    public int _Port = 6321;
    TcpListener _Server;
    bool _ServerStarted = false;

    public string _TestMessage = "Test 1234";


    private void Awake()
    {
        _Clients = new List<ServerClient>();
        _DisconnectList = new List<ServerClient>();

        try
        {
            _Server = new TcpListener(IPAddress.Any, _Port);
            _Server.Start();
            StartListening();
            _ServerStarted = true;

            _LocalIP = GetLocalIPAddress();
            Debug.Log("Server started: " + _LocalIP + ":" + _Port);
        }
        catch(Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }
    }

    private void Update()
    { 
        if (!_ServerStarted)
            return;

        foreach(ServerClient c in _Clients)
        {
            // Is the client still connected?
            if(!IsConnected(c._Tcp))
            {
                c._Tcp.Close();
                _DisconnectList.Add(c);
                continue;
            }
            // Check messages from this client
            else
            {
                NetworkStream stream = c._Tcp.GetStream();
                if(stream.DataAvailable)
                {
                    if (_ReadType == ReadType.ByteArray)
                    {
                        int count = stream.ReadByte();
                        byte[] byteData = new byte[count];
                        stream.Read(byteData, 0, byteData.Length);
                        if (byteData != null)
                            OnIncomingData(c, byteData);
                    }
                    else
                    {
                        StreamReader reader = new StreamReader(stream, true);
                        string data = reader.ReadLine();

                        if (data != null)
                            OnIncomingData(c, data);
                    }
                }
            }
        }

        //--  MANAGE DISCONNECTIONS
        for (int i = 0; i < _DisconnectList.Count - 1; i++)
        {
            Broadcast(_DisconnectList[i]._ClientName + " has disconnected.", _Clients);

            _Clients.Remove(_DisconnectList[i]);
            _DisconnectList.RemoveAt(i);
        }


        //--  DEBUG
        if (Input.GetKeyDown(KeyCode.T))
        {
            if(_ReadType == ReadType.ByteArray)
            {
                WriteBytes(new byte[] { 8, 7, 6, 4 });
                WriteBytes(new byte[] { 81, 71, 61, 41, 31, 91 });
                //BroadcastBytes(_Clients);
            }
            else
            {
                Broadcast(_TestMessage, _Clients);
            }
        }
    }

    private void LateUpdate()
    {
        if(_BytesList.Count > 0)
            BroadcastBytes(_Clients);
    }

    #region CONNECTION
    private bool IsConnected(TcpClient tcp)
    {
        try
        {
            if (tcp != null && tcp.Client != null && tcp.Client.Connected)
            {
                if (tcp.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(tcp.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }

                return true;
            }
            else
                return false;
        }
        catch
        {
            return false;
        }
    }

    private void StartListening()
    {
        _Server.BeginAcceptTcpClient(AcceptTcpClient, _Server);
    }

    void AcceptTcpClient(IAsyncResult asyncResult)
    {
        TcpListener listener = (TcpListener)asyncResult.AsyncState;

        _Clients.Add(new ServerClient(listener.EndAcceptTcpClient(asyncResult)));
        StartListening();

        //--  Send message to tell everyone a new client has joined
        Broadcast(_Clients[_Clients.Count - 1]._ClientName + " has connected", _Clients);
    }
    #endregion

    #region READ
    private void OnIncomingData(ServerClient c, string data)
    {
        Debug.Log(c._ClientName + " has sent: " + data);
    }

    private void OnIncomingData(ServerClient c, byte[] data)
    {
        Debug.Log(c._ClientName + " has sent data of length : " + data.Length);
        for (int i = 0; i < data.Length; i++)
        {
            Debug.Log(i + " - " + data[i]);
        }       
    }
    #endregion

    #region WRITE
    void Broadcast(string data, List<ServerClient> clients)
    {
        foreach (ServerClient c in clients)
        {
            try
            {
                StreamWriter writer = new StreamWriter(c._Tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error: " + e.Message + " to clients " + c._ClientName);
            }
        }
    }

    void BroadcastBytes(List<ServerClient> clients)
    {
        // Insert bytelist count at start of list
        _BytesList.Insert(0, (byte)_BytesList.Count);
        byte[] byteArray = _BytesList.ToArray();

        foreach (ServerClient c in clients)
        {
            try
            {
                Stream stream = c._Tcp.GetStream();
                stream.Write(byteArray, 0, byteArray.Length);
                stream.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error: " + e.Message + " to clients " + c._ClientName);
            }
        }

        _BytesList.Clear();
    }

    List<byte> _BytesList = new List<byte>();
    void WriteBytes(byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            _BytesList.Add(data[i]);
        }
    }

    public void WriteInts(uint[] intVals, bool littleEndian, bool debugPrint = true)
    {
        for (uint i = 0; i < intVals.Length; i++)
        {
            if (littleEndian)
            {
                _BytesList.AddRange(IntToLittleEndian(intVals[i]));
            }
            else
            {
                _BytesList.AddRange(System.BitConverter.GetBytes(intVals[i]));
            }
        }

        string debug = "";
        foreach (byte by in _BytesList)
        {
            debug += by.ToString() + ",";
        }

        if (debugPrint) Debug.Log(Time.timeSinceLevelLoad + "  -  Adding bytes array to stack: " + debug);
    }

    byte[] IntToLittleEndian(uint data)
    {
        byte[] b = new byte[2];
        b[0] = (byte)data;
        b[1] = (byte)((data >> 8) & 0xFF);
        return b;
    }
    #endregion

    public static string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
}

public class ServerClient
{
    public TcpClient _Tcp;
    public string _ClientName;

    public ServerClient(TcpClient clientSocket)
    {
        _ClientName = "Guest";
        _Tcp = clientSocket;
    }
}
