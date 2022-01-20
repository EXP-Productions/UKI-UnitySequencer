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
    public Dictionary<UkiActuatorAssignments, Actuator> _AllActuators = new Dictionary<UkiActuatorAssignments, Actuator>();

    // UI
    [HideInInspector]
    public ActuatorSlider[] _ActuatorSliders;

    [Header("UI - MAIN")]
    public Button _EStopButton;
    public UI_ButtonHold _IgnoreCollisionHoldButton;
    public Button _CalibrateButton;
    
    public TMPro.TMP_Dropdown _UKIModeDropDown;

    public GameObject _EstopWarning;
    public Image _HeartBeatDisplay;
    public Slider _OfflineSpeedScalerSlider;

    [Header("UI - ACTUATORS")]
    public Toggle _LeftSendToggle;
    public Toggle _RightSendToggle;

    [Header("UI - LIGHTING")]
    public Toggle _LightingSend;





    private void Awake()
    {
        Instance = this;
    }

   
    public bool _IgnoreCollisions = false;
    // Start is called before the first frame update
    void Start()
    {
        //-- LIGHTING UPDATE      
        _LightingSend.onValueChanged.AddListener((bool b) => UKILightingManager.Instance.UpdateLighting = b);
        UKILightingManager.Instance.UpdateLighting = false;

        //_EStopButton.onClick.AddListener(() => UkiCommunicationsManager.Instance.EStopButtonToggle());

        _IgnoreCollisionHoldButton._OnDown.AddListener(() => { UkiCommunicationsManager.Instance.EStopButtonToggle(); _IgnoreCollisions = true; });
        _IgnoreCollisionHoldButton._OnUp.AddListener(() => _IgnoreCollisions = false);

        _UKIModeDropDown.AddOptions(new List<string>() { "Simulation", "Sending UDP", "Sending TCP",  });
        _UKIModeDropDown.onValueChanged.AddListener((int i) => SetUKIModeFromDropDown(i));
        _UKIModeDropDown.SetValueWithoutNotify(0);

        _CalibrateButton.onClick.AddListener(UKI_PoseManager.Instance.CalibrationPose);

        _ActuatorSliders = FindObjectsOfType<ActuatorSlider>();

        SetUKIModeFromDropDown((int)UKIMode.Simulation);

        _OfflineSpeedScalerSlider.onValueChanged.AddListener((float f) => SetOfflineSpeedScaler(f));
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
        }
    }

    void SetUKIModeFromDropDown(int i)
    {
        // Enable sends for left and right
        if (i != (int)UKIMode.Simulation)
        {
            _LeftSendToggle.gameObject.SetActive(true);
            _RightSendToggle.gameObject.SetActive(true);
            _OfflineSpeedScalerSlider.transform.parent.gameObject.SetActive(false);
        }
        // Enable simulation scaler
        else
        {
            _LeftSendToggle.gameObject.SetActive(false);
            _RightSendToggle.gameObject.SetActive(false);
            _OfflineSpeedScalerSlider.transform.parent.gameObject.SetActive(true);
        }

        UkiCommunicationsManager.Instance.SetUKIMode(i);
    }


    public void AddActuator(Actuator actuator)
    {
        if (actuator._ActuatorIndex.ToString().Contains("Left") && actuator != null)
            _LeftActuators.Add(actuator);
        else if (actuator._ActuatorIndex.ToString().Contains("Right") && actuator != null)
            _RightActuators.Add(actuator);

        if (actuator != null)
            _AllActuators.Add(actuator._ActuatorIndex, actuator);
    }

    public void SetOfflineSpeedScaler(float f)
    {
        print("Offline speed scaler: " + f);

        foreach (KeyValuePair<UkiActuatorAssignments, Actuator> actuator in UKI_UIManager.Instance._AllActuators)
            actuator.Value._OfflineSpeedScaler = f;
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

 

    public void SetActuatorSliders(bool withoutNotify = false)
    {
        for (int i = 0; i < _ActuatorSliders.Length; i++)
            _ActuatorSliders[i].SetToActuatorNorm(withoutNotify);
    }

    public void MirrorRightToLeft()
    {
        for (int i = 0; i < _RightActuators.Count; i++)
        {
            string name = _RightActuators[i]._ActuatorIndex.ToString();
            name = name.Replace("Right", "Left");

            // Find the left actuator
            Actuator match = _LeftActuators.Find(x => x._ActuatorIndex.ToString() == name);
            match.TargetNormExtension = _RightActuators[i].TargetNormExtension;

            print("Match: " + _LeftActuators[i]._ActuatorIndex.ToString() + "    " + match._ActuatorIndex.ToString());
        }

        SetActuatorSliders();
    }

    public void MirrorLeftToRight()
    {
        for (int i = 0; i < _LeftActuators.Count; i++)
        {
            string name = _LeftActuators[i]._ActuatorIndex.ToString();
            name = name.Replace("Left", "Right");

            // Find the right actuator
            Actuator match = _RightActuators.Find(x => x._ActuatorIndex.ToString() == name);
            match.TargetNormExtension = _LeftActuators[i].TargetNormExtension;

            print("Match: " + _LeftActuators[i]._ActuatorIndex.ToString() + "    " + match._ActuatorIndex.ToString());
        }

        SetActuatorSliders();
    }

    void SetActuatorExtension(ActuatorSlider actuatorSlider)
    {
        actuatorSlider._Actuator.TargetNormExtension = actuatorSlider._Slider.value;
    }
}
