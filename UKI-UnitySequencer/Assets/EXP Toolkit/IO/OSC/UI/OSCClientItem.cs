using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSCClientItem : MonoBehaviour {
    public UnityEngine.UI.Button m_DelButton;
    public UnityEngine.UI.Text m_ClientEndPointText;
    public string m_IP;
    public string m_Port;
    private OSCUI m_OSCUI;
    
    void Awake()
    {
        m_DelButton.onClick.AddListener(() => DelClicked());
    }

    void DelClicked()
    {
        m_OSCUI.Delete(this);
    }

    public void Init(OSCUI oscUI, string ip, string port)
    {
        m_OSCUI = oscUI;
        m_IP = ip;
        m_Port = port;
        m_ClientEndPointText.text = ip + ":" + port;
    }
    
}
