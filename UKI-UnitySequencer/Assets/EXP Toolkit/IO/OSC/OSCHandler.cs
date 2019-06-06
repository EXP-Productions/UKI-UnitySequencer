//
//	  UnityOSC - Open Sound Control interface for the Unity3d game engine	  
//
//	  Copyright (c) 2012 Jorge Garcia Martin
//
// 	  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// 	  documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// 	  the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
// 	  and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// 	  The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// 	  of the Software.
//
// 	  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// 	  TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// 	  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// 	  CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// 	  IN THE SOFTWARE.
//
//	  Inspired by http://www.unifycommunity.com/wiki/index.php?title=AManagerClass

using System;
using System.Net;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

using UnityEngine;
using UnityOSC;

/// <summary>
/// Handles all the OSC servers and clients of the current Unity game/application.
/// Tracks incoming and outgoing messages.
/// 
/// 
/// FEATURE - make listener optional
/// </summary>
public class OSCHandler : MonoBehaviour
{
    public enum OSCMode
    {
        OSCIn = 0,
        OSCOut = 1
    }
    // Static singleton property
    static OSCHandler m_Instance { get; set; }

    // Static singleton property
    public static OSCHandler Instance
    {
        // Here we use the ?? operator, to return 'instance' if 'instance' does not equal null
        // otherwise we assign instance to a new component and return that
        get
        {
            if (m_Instance == null)
            {
                GameObject managerParent = GameObject.Find("*Managers");
                if (managerParent == null)
                    managerParent = new GameObject("*Managers");

                m_Instance = new GameObject("OSCHandlerNoGUI").AddComponent<OSCHandler>();
                m_Instance.ManualInstantiation();
                m_Instance.transform.parent = managerParent.transform;
            }

            return m_Instance;
        }
    }

    public bool _OSCInThisFrame;
    public bool _OSCOutThisFrame;

    public OSCServer m_Server;
    public int m_ServerPort = 9000;
    List<OSCClient> m_Clients = new List<OSCClient>();

    public List<OSCClient> Clients
    { get { return m_Clients; } }
    public int ClientCount
    {
        get
        {
            return m_Clients.Count;
        }
    }

    //public Server m_Server;
    public List<OSCListener> m_OSCListeners = new List<OSCListener>();
    public List<OSCListener> m_DoNoDestroyOSCListeners = new List<OSCListener>();
    List<string> m_OSCListenerAddresses = new List<string>();
    public Dictionary<string, Queue<OSCPacket>> m_OSCAll = new Dictionary<string, Queue<OSCPacket>>();
    private Dictionary<string, OSCPacket> m_SentMessageLog = new Dictionary<string, OSCPacket>();

    public bool _LoadOnStart = false;

    OSCSendRecieveGUI _OSCGUI;

    public Dictionary<string, OSCPacket> SentMessageLog
    {
        get { return m_SentMessageLog; }
    }

    private const int _loglength = 50;

    public void MuteOSC()
    {
        MuteOSCOut = !m_MuteOSCOut;
    }

    static bool m_MuteOSCOut = false;
    public bool MuteOSCOut
    {
        get { return m_MuteOSCOut; }

        set
        {
            if (m_MuteOSCOut != value)
            {
                m_MuteOSCOut = value;
                if (m_MuteOSCOut)
                    SendOSCMessage("/osc/mute", 1);
                else
                    SendOSCMessage("/osc/mute", 0);
            }
        }
    }

    public bool m_Debug = false;

    public bool m_DebugCreateNoServers = false;


    public void AddListener(OSCListener listener, bool doNotDestroyOnLoad)
    {
        if (!m_OSCListeners.Contains(listener))
        {
            if (listener.m_Address == null)
            {
                print("-- OSC: address is null");
            }
            else if (listener.m_Address == "")
            {
                print("-- OSC: Address is empty. Cannot register listener");
            }
            else if (listener.m_Address[0] != '/')
            {
                print("-- OSC: address does not contain leading slash");
            }
            else
                m_OSCListeners.Add(listener);
        }

        if (doNotDestroyOnLoad)
        {
            if (!m_DoNoDestroyOSCListeners.Contains(listener))
            {
                m_DoNoDestroyOSCListeners.Add(listener);
            }
        }
    }


    // OSC control listeners
    OSCListener m_OSCMute;
    void Awake()
    {
        if (m_Instance == null)
            m_Instance = this;
    }


    void Start()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
        m_OSCMute = new OSCListener("/osc/mute");		// Setup the osc muste listener
        Debug.Log("My IP: " + NetworkHelper.GetLocalIPAddress());

        if (_LoadOnStart)
            Load("");
        else
            StartServer(m_ServerPort);
        
        m_ServerPort = m_Server.LocalPort;
        tempPortIn = m_Server.LocalPort.ToString();	// Set the temp port in to display in GUI

        // Try find OSC GUI
        _OSCGUI = FindObjectOfType<OSCSendRecieveGUI>();
    }

    void ManualInstantiation()
    {
        m_Clients = new List<OSCClient>();
        if (m_OSCListeners == null)
            m_OSCListeners = new List<OSCListener>();
        if (m_DoNoDestroyOSCListeners == null)
            m_DoNoDestroyOSCListeners = new List<OSCListener>();
        if (m_OSCListenerAddresses == null)
            m_OSCListenerAddresses = new List<string>();
        if (m_OSCAll == null)
            m_OSCAll = new Dictionary<string, Queue<OSCPacket>>();
        if (m_SentMessageLog == null)
            m_SentMessageLog = new Dictionary<string, OSCPacket>();
    }


    // Clear all listeners when new level is loaded. Listeners that are in the do no destroy heirarchy will need to be added again
    void OnLevelFinishedLoading(Scene scene, LoadSceneMode loadSceneMode)
    {
        // Clear listener lists
        m_OSCListeners.Clear();
        m_OSCListenerAddresses.Clear();
        m_OSCAll.Clear();

        foreach (OSCListener oscListener in m_DoNoDestroyOSCListeners)
        {
            AddListener(oscListener, false);
        }
    }

    List<OSCPacket> newPacketsCopy = new List<OSCPacket>();
    void FixedUpdate()
    {
        if (m_OSCMute.Updated)		// Update osc muting from external OSC msg
        {
            if ((int)m_OSCMute.GetData(0) == 0)
            {
                print("Unmuted");
                MuteOSCOut = false;
            }
            else
            {
                print("muted");
                MuteOSCOut = true;
            }
        }

        // ------------------------ UPDATE RECIEVED PACKETS FROM SERVER		
        bool doLoop = true;

        // If it has a gui flash indicator
        if (_OSCGUI != null)
        {
            if(_OSCOutThisFrame)
                _OSCGUI.SendIndication();

            if(_OSCInThisFrame)
                _OSCGUI.RecieveIndication();
        }
        
        // Reset send and recieve this frame
        _OSCInThisFrame = false;
        _OSCOutThisFrame = false;

        while (doLoop)
        {
            OSCPacket packet = null;
            lock (m_Server.LastReceivedPackets)
            {
                if (m_Server.LastReceivedPackets.Count > 0)
                {
                    packet = m_Server.LastReceivedPackets.Dequeue();
                }
            }

            if (packet != null)
            {
                if(!m_OSCAll.ContainsKey(packet.Address))
                {
                    m_OSCAll.Add(packet.Address, new Queue<OSCPacket>());
                }
                m_OSCAll[packet.Address].Enqueue(packet);
                _OSCInThisFrame = true;
            }
            else
            {
                doLoop = false;
            }
        }

        foreach (OSCClient client in m_ClientsToRemove)	// Remove any clients that need to be removed
        {
            Debug.Log("OSC: Client removed " + client.ClientIPAddress.ToString());
            m_Clients.Remove(client);
        }
        m_ClientsToRemove.Clear();

    }

    public void Save(string prefix)
    {
        prefix = "EXPOSC." + prefix;
        PlayerPrefs.SetInt(prefix + "_clients.Count", m_Clients.Count);
        print("OSC - Saving " + m_Clients.Count + " clients.");
        PlayerPrefs.SetInt(prefix + "serverPort", m_ServerPort);

        int clientCount = 0;
        foreach (OSCClient client in m_Clients)
        {
            string ip = client.ClientIPAddress.ToString();
            print("OSC: " + ip + " saved");
            PlayerPrefs.SetString(prefix + "clientIP" + clientCount, ip);
            PlayerPrefs.SetInt(prefix + "clientPort" + clientCount, client.Port);

            clientCount++;
        }
    }

    public void Load(string prefix)
    {
        prefix = "EXPOSC." + prefix;
        m_Clients.Clear();
        StopAllServers();

        m_ServerPort = PlayerPrefs.GetInt(prefix + "serverPort", 9000);

        int clientCount = PlayerPrefs.GetInt(prefix + "_clients.Count");
        print("OSC - Loading " + clientCount + " clients.");

        for (int i = 0; i < clientCount; i++)
        {
            string ip = PlayerPrefs.GetString(prefix + "clientIP" + i);
            int port = PlayerPrefs.GetInt(prefix + "clientPort" + i);
            AddNewClient(ip, port.ToString());
        }

        StartServer(m_ServerPort);
    }
     
    #region Methods
    /// <summary>
    /// Ensure that the instance is destroyed when the game is stopped in the Unity editor
    /// Close all the OSC clients and servers
    /// </summary>
    public void OnApplicationQuit()
    {
        Save("");

        StopOSCHandler();

    }

    public void StopOSCHandler()
    {
        StopAllClients();
        StopAllServers();

        m_Instance = null;
    }

    void StopAllClients()
    {
        foreach (OSCClient client in m_Clients)
        {
            client.Close();
        }
    }

    bool StopAllServers()
    {
        if (m_Server != null)
        {
            if (m_Server.UDPClient != null)
                m_Server.Close();
        }
        else
        {
            print("No servers found");
            return false;
        }

        print("OSC:  Server closed");

        return true;
    }

    /// <summary>
    /// Creates an OSC Client (sends OSC messages) given an outgoing port and address.
    /// </summary>
    /// <param name="clientId">
    /// A <see cref="System.String"/>
    /// </param>
    /// <param name="destination">
    /// A <see cref="IPAddress"/>
    /// </param>
    /// <param name="port">
    /// A <see cref="System.Int32"/>
    /// </param>
    public void CreateClient(string clientId, IPAddress destination, int port)
    {
        if (m_DebugCreateNoServers)
            return;

        OSCClient newOSCClient = new OSCClient(destination, port);
        m_Clients.Add(newOSCClient);

        // Send test message
        string testaddress = "/test/alive/";
        OSCMessage message = new OSCMessage(testaddress, destination.ToString());
        message.Append(port); message.Append("OK");
        newOSCClient.Send(message);

        Debug.Log("Client created. " + clientId + ", " + destination.ToString() + ", " + port.ToString() + " Client count: " + m_Clients.Count);
    }


    /// <summary>
    /// Creates an OSC Server (listens to upcoming OSC messages) given an incoming port.
    /// </summary>	

    bool m_ServerInit = false;
    public bool ServerInit
    { get { return m_ServerInit; } }

    public bool StartServer(int port)
    {
        if (m_DebugCreateNoServers)
            return false;

        if (m_Server != null)
            StopAllServers();

        m_Server = new OSCServer(port);
        Debug.Log("OSC:  Server created.");
        m_ServerInit = !m_Server.PortUnavailable;

        return !m_Server.PortUnavailable;
    }

    void SendMessageToAllClients(OSCMessage oscMessage)
    {
        if (m_DebugCreateNoServers) return;

        if (m_MuteOSCOut) return;       

        int clientCount = m_Clients.Count;

        if(clientCount > 0)
            _OSCOutThisFrame = true;

        for (int i = 0; i < m_Clients.Count; i++)
        {
            if (!m_Clients[i].IsSent(oscMessage))
            {
                m_Clients.Remove(m_Clients[i]);
            }
        }

        if (m_LogOutput)
        {
            if (!m_SentMessageLog.ContainsKey(oscMessage.Address))
            {
                m_SentMessageLog.Add(oscMessage.Address, oscMessage);
            }
            else
            {
                m_SentMessageLog[oscMessage.Address] = oscMessage;
            }
        }
    }

    public void SendOSCMessage(OSCMessage oscMessage)
    {
        SendMessageToAllClients(oscMessage);
    }

    public void SendOSCMessage(string address, float val)
    {
        UnityOSC.OSCMessage newMsg = new UnityOSC.OSCMessage(address, val);
        SendMessageToAllClients(newMsg);
    }

    public void SendOSCMessage(string address, int val)
    {
        UnityOSC.OSCMessage newMsg = new UnityOSC.OSCMessage(address, val);
        SendMessageToAllClients(newMsg);
    }

    public void SendBlankOSCMessage(string address)
    {
        UnityOSC.OSCMessage newMsg = new UnityOSC.OSCMessage(address);
        SendMessageToAllClients(newMsg);
    }

    public void SendOSCMessage(string address, string val)
    {
        UnityOSC.OSCMessage newMsg = new UnityOSC.OSCMessage(address, val);
        SendMessageToAllClients(newMsg);
    }

    public void SendOSCMessage(string address, byte[] bytes)
    {
        UnityOSC.OSCMessage newMsg = new UnityOSC.OSCMessage(address, bytes);
        SendMessageToAllClients(newMsg);
    }

    public void SendOSCMessage(string address, Vector2 vec2)
    {
        UnityOSC.OSCMessage newMsg = new UnityOSC.OSCMessage(address);
        newMsg.AppendFloat(vec2.x);
        newMsg.AppendFloat(vec2.y);
        SendMessageToAllClients(newMsg);
    }

    public void SendOSCMessage(string address, Vector3 vec3)
    {
        UnityOSC.OSCMessage newMsg = new UnityOSC.OSCMessage(address);
        newMsg.AppendFloat(vec3.x);
        newMsg.AppendFloat(vec3.y);
        newMsg.AppendFloat(vec3.z);
        SendMessageToAllClients(newMsg);
    }

    public bool m_LogOutput = false;


    public static bool m_LockNewPackets = false;
    public static bool m_LockNewPackets2 = false;
    List<OSCPacket> newPackets = new List<OSCPacket>();


    /// <summary>
    /// Converts a collection of object values to a concatenated string.
    /// </summary>
    /// <param name="data">
    /// A <see cref="List<System.Object>"/>
    /// </param>
    /// <returns>
    /// A <see cref="System.String"/>
    /// </returns>
    private string DataToString(List<object> data)
    {
        string buffer = "";

        for (int i = 0; i < data.Count; i++)
        {
            buffer += data[i].ToString() + " ";
        }

        buffer += "\n";

        return buffer;
    }

    /// <summary>
    /// Formats a milliseconds number to a 000 format. E.g. given 50, it outputs 050. Given 5, it outputs 005
    /// </summary>
    /// <param name="milliseconds">
    /// A <see cref="System.Int32"/>
    /// </param>
    /// <returns>
    /// A <see cref="System.String"/>
    /// </returns>
    private string FormatMilliseconds(int milliseconds)
    {
        if (milliseconds < 100)
        {
            if (milliseconds < 10)
                return String.Concat("00", milliseconds.ToString());

            return String.Concat("0", milliseconds.ToString());
        }

        return milliseconds.ToString();
    }

    #endregion

    string GetTimeString(long timestamp)
    {
        string time = DateTime.FromFileTimeUtc(timestamp).Minute.ToString() + "."
                            + DateTime.FromFileTimeUtc(timestamp).Second.ToString() + "." + DateTime.FromFileTimeUtc(timestamp).Millisecond.ToString();

        return time;
    }

    public void RestartServerWithNewPort(string port)
    {
        m_ServerPort = 8000;
        Int32.TryParse(port, out m_ServerPort);

        Debug.Log("Resetarting server with new port: " + m_ServerPort);

        StopAllServers();

        if (!StartServer(m_ServerPort))
        {
            Debug.Log("Server NOT created with new port: " + m_ServerPort);
        }
        else
        {
            Debug.Log("Server created with new port: " + m_ServerPort);
        }
    }

    public void AddNewClient(string ip, string port)
    {
        if (m_Clients.Count >= 6)
        {
            Debug.Log("Cannot add any more clients");
            return;
        }

        string name = "Client" + m_Clients.Count;
        int portInt = Int32.Parse(port);
        CreateClient(name, IPAddress.Parse(ip), portInt);
        tempNewClientPort = port;
    }

    public void RemoveClient(string ip, string port)
    {
        OSCClient matchingClient = m_Clients.Find(s => s.ClientIPAddress.ToString() == ip && s.Port.ToString() == port);
        if (matchingClient != null)
        {
            matchingClient.Close();
            m_ClientsToRemove.Add(matchingClient);
        }
    }

    List<OSCClient> m_ClientsToRemove = new List<OSCClient>();
    void RemoveClient(OSCClient client)
    {
        client.Close();
        m_ClientsToRemove.Add(client);
    }

    public void RemoveAllClients()
    {
        foreach (OSCClient oscClient in m_Clients)
        {
            oscClient.Close();
            m_ClientsToRemove.Add(oscClient);
        }
    }

    public bool m_LogListeners = false;
    public bool m_LogInput = true;

    Vector2 scrollPosition = Vector2.zero;
    Vector2 scrollPosition1 = Vector2.zero;

    string tempPortIn = "8001";
    string tempNewClientPort = "9000";


}

