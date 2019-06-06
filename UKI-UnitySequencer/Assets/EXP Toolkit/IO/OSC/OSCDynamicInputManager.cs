using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Threading;

public class OSCDynamicInputManager : MonoBehaviour
{
    OSCListener m_DynamicInputListener;

    private List<OSCInputMapping> m_OSCInputMappings = new List<OSCInputMapping>();

    Queue<OSCMapMessage> OSCQueue = new Queue<OSCMapMessage>();
    //path to the midi config file.
    public string m_MappingConfigLocation = "\\Config\\OSCMapping.xml";

    bool m_ConfigLoaded = false;
    int m_OutputStatePeriod = 1;
    public bool m_UsePeriodicOutput = false;
    public float m_PeriodicOutputInterval = 10.0f;
    private float m_OutputTimer = 0.0f;

    void Awake()
    {
        m_DynamicInputListener = new OSCListener("/dynamicinput/");
        m_DynamicInputListener.m_ListenForChildren = true;
        LoadMappingConfig();
    }

    void Start()
    {
        m_OutputTimer = m_PeriodicOutputInterval;
    }

    void Update()
    {
        ProcessPendingInputMessages();

        for (int i = 0; i < OSCQueue.Count; i++)
        {
            OSCMapMessage oscMsg = OSCQueue.Dequeue();
            ProcessOSCIn(oscMsg.m_Address, oscMsg.m_Value);
        }

        if (!m_UsePeriodicOutput || m_OutputTimer <= 0.0f)
        {
            OutputCurrentState();
            m_OutputTimer = m_OutputStatePeriod;
        }
        else
        {
            m_OutputTimer -= Time.deltaTime;
        }
    }


    public void OutputCurrentState()
    {
        foreach (OSCInputMapping map in m_OSCInputMappings)
        {
            try
            {
                if (map.m_AssignmentType == "Property" && map.m_FeedbackState)
                {
                    float val = map.GetCurrentValue();
                    if (val != float.MinValue)
                    {
                        string address = map.m_MapAddress;
                        UnityOSC.OSCMessage msg = new UnityOSC.OSCMessage(address);
                        msg.AppendFloat(val);
                        OSCHandler.Instance.SendOSCMessage(msg);
                    }
                }
            }
            catch (Exception e)
            {
                MonoBehaviour.print(e.ToString());
            }
        }
    }

    #region "Load Config"

    void LoadMappingConfig()
    {
        try
        {
            string configLocation = Directory.GetCurrentDirectory();
            configLocation = configLocation + m_MappingConfigLocation;
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(configLocation);

            XmlElement rootElement = (XmlElement)xdoc.FirstChild;
            XmlNodeList mappingNodes = rootElement.GetElementsByTagName("Mapping");

            foreach (XmlElement xE in mappingNodes)
            {
                string address, className, assignmentType, assignmentName, IDType, gameObjectName;
                float minValue, maxValue;
                bool feedbackState, supportsInput;
                address = xE.Attributes["Address"].Value;

                className = xE.Attributes["ClassName"].Value;
                IDType = xE.Attributes["IDType"].Value;
                assignmentType = xE.Attributes["AssignmentType"].Value;
                assignmentName = xE.Attributes["AssignmentName"].Value;
                gameObjectName = xE.Attributes["GameObjectName"].Value;
                feedbackState = bool.Parse(xE.Attributes["FeedbackState"].Value);
                minValue = float.Parse(xE.Attributes["Min"].Value);
                maxValue = float.Parse(xE.Attributes["Max"].Value);
                supportsInput = bool.Parse(xE.Attributes["SupportsInput"].Value);

                OSCInputMapping inputMapping = new OSCInputMapping(address, className, gameObjectName, IDType, assignmentType, assignmentName, minValue, maxValue, feedbackState, supportsInput);
                if (inputMapping.m_Mapped)
                {
                    m_OSCInputMappings.Add(inputMapping);
                }
                else
                {
                    //might as well free some memory
                    inputMapping = null;
                }
            }

            m_ConfigLoaded = true;
        }
        catch (Exception e)
        {
            print(e.ToString());
        }
    }

    #endregion

    #region "OSC Input Handling"
    void ProcessPendingInputMessages()
    {
        try
        {
            if (m_DynamicInputListener.Updated)
            {
                while (m_DynamicInputListener.DataAvailable == true)
                {
                    KeyValuePair<string, List<object>> kp = m_DynamicInputListener.m_Data.Dequeue();
                    string fullAddress = kp.Key;
                    string truncatedAddress = kp.Key.Replace(m_DynamicInputListener.m_Address, "");
                    float inputValue = (float)kp.Value[0];
                    lock (OSCQueue)
                    { OSCQueue.Enqueue(new OSCMapMessage(truncatedAddress, inputValue)); }
                }
            }
        }
        catch (Exception e)
        { print(e.ToString()); }
    }

    void ProcessOSCIn(string address, float value)
    {
        foreach (OSCInputMapping map in m_OSCInputMappings)
        {
            if (map.m_MapAddress == address && map.m_SupportsInput)
            {
                map.UpdateValue(value);
            }
        }
    }


}
    #endregion

#region "Supporting Classes"


public class OSCMapMessage
{
    public string m_Address;
    public float m_Value;
    public OSCMapMessage(string address, float value)
    {
        m_Address = address;
        m_Value = value;
    }
}

public class OSCInputMapping
{
    public string m_ClassName;
    public string m_GameObjectName;
    public string m_IDType;

    public string m_AssignmentType;
    public string m_AssignmentName;

    public float m_MinValue;
    public float m_MaxValue;
    public bool m_Mapped = false;

    private object m_TargetObject;
    private PropertyInfo m_PropertyInfo;
    private MethodInfo m_MethodInfo;
    public bool m_FeedbackState;
    public bool m_SupportsInput;
    public string m_MapAddress;


    public OSCInputMapping(string mapAddress, string className, string gameObjectName, string idType, string assignmentType, string assignmentName,
        float minValue, float maxValue, bool feedbackState, bool supportsInput)
    {
        m_ClassName = className;
        m_AssignmentName = assignmentName;
        m_IDType = idType;
        m_AssignmentType = assignmentType;
        m_GameObjectName = gameObjectName;
        m_MinValue = minValue;
        m_MaxValue = maxValue;
        m_MapAddress = mapAddress;
        m_FeedbackState = feedbackState;
        m_SupportsInput = supportsInput;

        Type t = Type.GetType(m_ClassName, false, true);

        //the configurable midi stuff will assigns the target object in two ways
        //IDType = ClassName: The first object of type found in the scene.
        //IDType = SceneName: The first object of type and name found in the scene
        UnityEngine.Object[] o = UnityEngine.Resources.FindObjectsOfTypeAll(t);
        if (t == null || o.Length == 0)
        {
            m_Mapped = false;
        }
        else
        {
            if (o.Length > 0)
            {
                if (m_IDType == "ClassName")
                {
                    m_TargetObject = (object)o[0];
                }
                else if (m_IDType == "GameObjectName")
                {
                    foreach (UnityEngine.Object obj in o)
                    {
                        if (obj.name == m_GameObjectName)
                        {
                            m_TargetObject = obj;
                            break;
                        }
                    }
                }
            }

            if (m_AssignmentType == "Property")
            {
                PropertyInfo[] pInfos = t.GetProperties();
                foreach (PropertyInfo pInfo in pInfos)
                {
                    if (pInfo.Name == m_AssignmentName && pInfo.CanWrite)
                    {
                        m_PropertyInfo = pInfo;
                        m_Mapped = true;
                    }
                }
            }
            else if (m_AssignmentType == "Method")
            {
                MethodInfo[] mInfos = t.GetMethods();
                foreach (MethodInfo mInfo in mInfos)
                {
                    if (mInfo.Name == m_AssignmentName && mInfo.IsPublic)
                    {
                        m_MethodInfo = mInfo;
                        m_Mapped = true;
                    }
                }
            }
        }
    }

    public float GetCurrentValue()
    {
        float returnVal = float.MinValue;
        try
        {
            var tryVal = m_PropertyInfo.GetValue(m_TargetObject, null);
            returnVal = (float)tryVal;
        }
        catch (Exception e)
        { MonoBehaviour.print(e.ToString()); }
        return returnVal;
    }


    public void UpdateValue(float value)
    {
        try
        {
            //FEATURE: need conditions to handle bools/int/etc here. Just floats supported atm.
            if (m_AssignmentType == "Property")
            {
                float newValue = FloatExtensions.Scale(value, 0f, 1.0f, m_MinValue, m_MaxValue);
                m_PropertyInfo.SetValue(m_TargetObject, newValue, null);
            }
            else if (m_AssignmentType == "Method")
            {
                if (m_MaxValue == m_MinValue && m_MinValue == -1.0f)
                {
                    m_MethodInfo.Invoke(m_TargetObject, null);
                }
                else
                {
                    m_MethodInfo.Invoke(m_TargetObject, new object[1] { (int)m_MaxValue });
                }
            }
        }
        catch
        { }
    }
#endregion

}