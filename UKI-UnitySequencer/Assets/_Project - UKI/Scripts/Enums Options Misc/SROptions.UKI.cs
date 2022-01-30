using System.ComponentModel;
using UnityEngine;

public partial class SROptions
{
    // Default Value for property
    private float _ActuatorArrivalRange = 3f;
    private float _RequiredReadyCount = 23;

    // Options will be grouped by category
    [NumberRange(0, 10)]   
    [Category("Animation")]
    public float ActuatorArrivalRange
    {
        get { return _ActuatorArrivalRange; }
        set { _ActuatorArrivalRange = value; }
    }

    [NumberRange(0, 100)]
    [Category("Animation")]
    public int RequiredReadyCount
    {
        get { return (int)_RequiredReadyCount; }
        set { _RequiredReadyCount = value; UKI_PoseManager.Instance._RequiredReadyCount = (int)_RequiredReadyCount; }
    }

    public void PrintActuatorPositions()
    {
        UKI_PoseManager.Instance.PrintAllActuatorValues();
    }

    public void PrintActuatorsOutOfRange()
    {
        UKI_PoseManager.Instance.PrintAllActuatorOutOfRanges();
    }

    private bool _DebugUDP;
    private bool _DebugActuatorInternal;
    private bool _DebugCommSend;
    private bool _DebugCommRecieve;

    [Category("Comms")]
    public bool DebugCommsSend
    {
        get { return _DebugCommSend; }
        set
        {
            _DebugCommSend = value;
            UkiCommunicationsManager.Instance._DebugSend = _DebugCommSend;
        }
    }

    [Category("Comms")]
    public bool DebugCommsRecieve
    {
        get { return _DebugCommRecieve; }
        set
        {
            _DebugCommRecieve = value;
            UkiCommunicationsManager.Instance._DebugRecieve = _DebugCommRecieve;
        }
    }

    [Category("Comms")]
    public bool DebugUDP
    {
        get { return _DebugUDP; }
        set
        {
            _DebugUDP = value;
            UkiCommunicationsManager.Instance._DebugUDP = _DebugUDP;
        }
    }

    [Category("Comms")]
    public bool DebugActuatorInternal
    {
        get { return _DebugActuatorInternal; }
        set
        {
            _DebugActuatorInternal = value;
            UkiCommunicationsManager.Instance._DebugActuatorInternal = _DebugActuatorInternal;
        }
    }

    [Category("Comms")]
    public string TCPHostIPAddress
    {
        get { return Client.Instance._HostIP; }
        set
        {
            Client.Instance._HostIP = value;
        }
    }

    [Category("Comms")]
    public int TCPHostPort
    {
        get { return Client.Instance._Port; }
        set
        {
            Client.Instance._Port = value;
        }
    }

    [Category("Comms")]
    public void ConnectoToTCPHost()
    {
        Client.Instance.ConnectToServer();
    }

    //[Category("Comms")]
    //public int ConnectToTCPHost
    //{
    //    get { return Client.Instance._Port; }
    //    set
    //    {
    //        Client.Instance._Port = value;
    //    }
    //}






    /*
    private float _myRangeProperty = 0f;

    // The NumberRange attribute will ensure that the value never leaves the range 0-10
    [NumberRange(0, 10)]
    [Category("My Category")]
    public float MyRangeProperty
    {
        get { return _myRangeProperty; }
        set { _myRangeProperty = value; }
    }
    */

}
