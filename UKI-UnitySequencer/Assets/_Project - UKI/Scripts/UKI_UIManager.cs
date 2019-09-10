using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UKI_UIManager : MonoBehaviour
{
    public static UKI_UIManager Instance;
    
    [Header("Actuators")]    
    public List<Actuator> _LeftActuators = new List<Actuator>();
    public List<Actuator> _RightActuators = new List<Actuator>();
    public List<Actuator> _AllActuators = new List<Actuator>();

    // UI
    [HideInInspector]
    public ActuatorSlider[] _ActuatorSliders;

    [Header("UI - MAIN")]
    public Button _EStopButton;
    public UI_ButtonHold _IgnoreCollisionHoldButton;
    public Button _CalibrateButton;
    public Toggle _SendToModBusToggle;
    public GameObject _EstopWarning;
    public Image _HeartBeatDisplay;
    public Slider _OfflineSpeedScalerSlider;
    public Toggle _OfflineSimModeToggle;

    [Header("UI - ACTUATORS")]
    public Toggle _LeftEnabledToggle;
    public Toggle _RightEnabledToggle;
    
   

    

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            // Send EStop
            if (Input.GetKey(KeyCode.E))
            {
                UkiCommunicationsManager.Instance.EStop("ESTOP sent from C2");
            }

            // Raise wings
            if (Input.GetKey(KeyCode.W) && Input.GetKeyDown(KeyCode.UpArrow))
            {
                HackWings(true);
            }

            // Lower wings
            if (Input.GetKey(KeyCode.W) && Input.GetKeyDown(KeyCode.DownArrow))
            {
                HackWings(false);
            }

            // Toggle loop poses
            if (Input.GetKeyDown(KeyCode.L))
            {
                UKI_PoseManager.Instance._LoopPoses = !UKI_PoseManager.Instance._LoopPoses;
            }

            // Raise ankles
            if (Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.UpArrow))
            {
                HackAnkles(true);
            }

            // Lower ankles
            if (Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.DownArrow))
            {
                HackAnkles(false);
            }

            // Raise butt
            if (Input.GetKey(KeyCode.B) && Input.GetKeyDown(KeyCode.UpArrow))
            {
                ButtHack(true);
            }

            // Lower butt
            if (Input.GetKey(KeyCode.B) && Input.GetKeyDown(KeyCode.DownArrow))
            {
                ButtHack(false);
            }

        }
    }

    void HackWings(bool raise)
    {
        UKI_PoseManager.Instance._MaskWings = true;

        foreach (Actuator actuator in _AllActuators)
        {
            if (actuator._ActuatorIndex == UkiActuatorAssignments.RightWingRaise || actuator._ActuatorIndex == UkiActuatorAssignments.LeftWingRaise)
            {
                if (raise)
                {
                    actuator.NormExtension += 0.1f;
                }
                else
                {
                    actuator.NormExtension -= 0.1f;
                }
            }
        }
    }

    void HackAnkles(bool raise)
    {
        foreach (Actuator actuator in _AllActuators)
        {
            if (actuator._ActuatorIndex == UkiActuatorAssignments.LeftFrontAnkle || actuator._ActuatorIndex == UkiActuatorAssignments.LeftMidAnkle || actuator._ActuatorIndex == UkiActuatorAssignments.LeftRearAnkle || actuator._ActuatorIndex == UkiActuatorAssignments.RightFrontAnkle || actuator._ActuatorIndex == UkiActuatorAssignments.RightMidAnkle || actuator._ActuatorIndex == UkiActuatorAssignments.RightRearAnkle)
            {
                if (raise)
                {
                    actuator.NormExtension = 1.0f;
                }
                else
                {
                    actuator.NormExtension = 0.0f;
                }
            }
        }
    }

    void ButtHack(bool raise)
    {
        foreach (Actuator actuator in _AllActuators)
        {
            if (actuator._ActuatorIndex == UkiActuatorAssignments.Arse)
            {
                if (raise)
                {
                    actuator.NormExtension = 1.0f;
                }
                else
                {
                    actuator.NormExtension = 0.0f;
                }
            }
        }
    }

    public bool _IgnoreCollisions = false;
    // Start is called before the first frame update
    void Start()
    {
        //_EStopButton.onClick.AddListener(() => UkiCommunicationsManager.Instance.EStopButtonToggle());

        _IgnoreCollisionHoldButton._OnDown.AddListener(() => { UkiCommunicationsManager.Instance.EStopButtonToggle(); _IgnoreCollisions = true; });
        _IgnoreCollisionHoldButton._OnUp.AddListener(() => _IgnoreCollisions = false);

        _SendToModBusToggle.onValueChanged.AddListener(delegate { UkiCommunicationsManager.Instance.SendToModbusToggle(_SendToModBusToggle); });
        _SendToModBusToggle.isOn = UkiCommunicationsManager.Instance._SendToModbus;      

       // _MirrorLeftButton.onClick.AddListener(() => MirrorLeft());
       // _MirrorRightButton.onClick.AddListener(() => MirrorRight());

        _CalibrateButton.onClick.AddListener(CalibrateActuators);

      
        _ActuatorSliders = FindObjectsOfType<ActuatorSlider>();

   

        _OfflineSpeedScalerSlider.onValueChanged.AddListener((float f) => SetOfflineSpeedScaler(f));

     

    }

    public void AddActuator(Actuator actuator)
    {
        if (actuator._ActuatorIndex.ToString().Contains("Left") && actuator != null)
            _LeftActuators.Add(actuator);
        else if (actuator._ActuatorIndex.ToString().Contains("Right") && actuator != null)
            _RightActuators.Add(actuator);

        if (actuator != null)
            _AllActuators.Add(actuator);
    }

    public void SetOfflineSpeedScaler(float f)
    {
        print("Offline speed scaler: " + f);

        foreach (Actuator act in _AllActuators)
            act._OfflineSpeedScaler = f;
    }

    public void UpdateEstopButton()
    {
        if(UkiCommunicationsManager.Instance._EStopping)
        {
            _EStopButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Reset EStop";
        }
        else
        {
            _EStopButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "EStop";
        }
    }

 

    public void SetActuatorSliders()
    {
        for (int i = 0; i < _ActuatorSliders.Length; i++)
            _ActuatorSliders[i].SetToActuatorNorm();
    }

    public void MirrorRight()
    {
        for (int i = 0; i < _RightActuators.Count; i++)
        {
            _LeftActuators[i].NormExtension = _RightActuators[i].NormExtension;
        }

        SetActuatorSliders();
    }

    public void MirrorLeft()
    {
        for (int i = 0; i < _LeftActuators.Count; i++)
        {
            _RightActuators[i].NormExtension = _LeftActuators[i].NormExtension;
        }

        SetActuatorSliders();
    }

    void SetActuatorExtension(ActuatorSlider actuatorSlider)
    {
        actuatorSlider._Actuator.NormExtension = actuatorSlider._Slider.value;
    }

    void CalibrateActuators()
    {
        foreach (Actuator actuator in FindObjectsOfType<Actuator>())
        {
            actuator.Calibrate();
        }
    }

   
}
