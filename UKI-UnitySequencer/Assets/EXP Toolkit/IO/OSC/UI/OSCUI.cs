using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OSCUI : MonoBehaviour {

    public bool m_ServerInited;
    public UnityEngine.UI.Text m_TextOSCServer;
    public UnityEngine.UI.InputField m_InputOSCPort;
    public GameObject m_OSCActiveClients;
    public UnityEngine.UI.InputField m_InputOSCClientIP;
    public UnityEngine.UI.InputField m_InputOSCClientPort;

    public UnityEngine.UI.Button m_CloseButton;
    public UnityEngine.UI.Button m_SetPortButton;
    public UnityEngine.UI.Button m_AddClientButton;

    public GameObject m_ClientItemPrefab;
    private int m_ClientCount = 0;

    // Use this for initialization
    void Start()
    {
        m_SetPortButton.onClick.AddListener(SetNewPort);
        m_AddClientButton.onClick.AddListener(AddClient);
        m_CloseButton.onClick.AddListener(Close);
    }

    void Close()
    {
        this.gameObject.SetActive(false);
    }

    void Update()
    {
        if(!m_ServerInited && (OSCHandler.Instance.ServerInit))
        {
            UpdateSettingsGUI();
            m_ServerInited = true;
        }

        if (m_ClientCount != OSCHandler.Instance.ClientCount)
        {
            OSCClientItem[] clientItems = m_OSCActiveClients.GetComponentsInChildren<OSCClientItem>();
            foreach (OSCClientItem clientitem in clientItems)
            {
                Destroy(clientitem.gameObject);
            }

            foreach (UnityOSC.OSCClient client in OSCHandler.Instance.Clients)
            {
                AddClientItem(client.ClientIPAddress.ToString(), client.Port.ToString());
            }
            m_ClientCount = OSCHandler.Instance.ClientCount;
        }
    }

    void UpdateSettingsGUI()
    {
        m_InputOSCPort.text = OSCHandler.Instance.m_Server.LocalPort.ToString();
        m_TextOSCServer.text = NetworkHelper.GetLocalIPAddress();
    }

    public void AddClient()
    {
        int port;
        System.Net.IPAddress tempAddress; 
        if(System.Net.IPAddress.TryParse(m_InputOSCClientIP.text, out tempAddress) && int.TryParse(m_InputOSCClientPort.text, out port))
        {
            OSCHandler.Instance.AddNewClient(m_InputOSCClientIP.text, m_InputOSCClientPort.text);
            m_InputOSCClientPort.text = "";
            m_InputOSCClientIP.text = "";
        }
    }

    void AddClientItem(string ip, string port)
    {
        GameObject go = Instantiate(m_ClientItemPrefab);
        OSCClientItem oci = go.GetComponentInChildren<OSCClientItem>();
        oci.Init(this, ip, port);
        go.transform.parent = m_OSCActiveClients.transform;
        go.transform.localScale = new Vector3(1, 1, 1);
    }

    public void Delete(OSCClientItem item)
    {
        OSCHandler.Instance.RemoveClient(item.m_IP, item.m_Port);
        Destroy(item.transform.gameObject);
    }

    public void SetNewPort()
    {
        int port;
        if (int.TryParse(m_InputOSCPort.text, out port))
        {
            OSCHandler.Instance.RestartServerWithNewPort(m_InputOSCPort.text);
        }
        UpdateSettingsGUI();
    }
}
