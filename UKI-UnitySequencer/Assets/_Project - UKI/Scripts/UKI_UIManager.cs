using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UKI_UIManager : MonoBehaviour
{
    public static UKI_UIManager Instance;
    
    [Header("Actuators")]    
    public List<TestActuator> _LeftActuators = new List<TestActuator>();
    public List<TestActuator> _RightActuators = new List<TestActuator>();
    public List<TestActuator> _AllActuators = new List<TestActuator>();


    [Header("UI")]
    // UI - CAMERA
    public Slider _Slider_CamRot;
    public Slider _Slider_CamZ;

    // UI
    [HideInInspector]
    public ActuatorSlider[] _ActuatorSliders;

    public Button _EStopButton;
    public Button _CalibrateButton;
    public Toggle _SendToModBusToggle;
    public GameObject _EstopWarning;
    public Image _HeartBeatDisplay;

    public Button _MirrorLeftButton;
    public Button _MirrorRightButton;

    public RectTransform _PoseButtonParent;
    public Button _SelectPoseButtonPrefab;

    public Toggle _OfflineSimModeToggle;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _SendToModBusToggle.isOn = UkiCommunicationsManager.Instance._SendToModbus;

        _EStopButton.onClick.AddListener(() => UkiCommunicationsManager.Instance.EStopButtonToggle());
        _SendToModBusToggle.onValueChanged.AddListener(delegate { UkiCommunicationsManager.Instance.SendToModbusToggle(_SendToModBusToggle); });
        _OfflineSimModeToggle.onValueChanged.AddListener((bool b) => ToggleOfflineSimMode(b));

        _MirrorLeftButton.onClick.AddListener(() => MirrorLeft());
        _MirrorRightButton.onClick.AddListener(() => MirrorRight());

        _CalibrateButton.onClick.AddListener(CalibrateActuators);

        _ActuatorSliders = FindObjectsOfType<ActuatorSlider>();
    }

    public void AddActuator(TestActuator actuator)
    {
        if (actuator._ActuatorIndex.ToString().Contains("Left") && actuator != null)
            _LeftActuators.Add(actuator);
        else if (actuator._ActuatorIndex.ToString().Contains("Right") && actuator != null)
            _RightActuators.Add(actuator);

        if (actuator != null)
            _AllActuators.Add(actuator);
    }

    // Update is called once per frame
    void Update()
    {
        //_CamParent.SetLocalRotY(_CamYRotNorm * -360);
        //_Camera.SetLocalZ(_CamZNorm.ScaleFrom01(_CamZRange.x, _CamZRange.y));
    }

    void ToggleOfflineSimMode(bool b)
    {
        for (int i = 0; i < _AllActuators.Count; i++)
        {
            _AllActuators[i]._DEBUG_NoModBusSimulationMode = b;
        }
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

    public void AddPoseButton(int index)
    {
        Button newBtn = Instantiate(_SelectPoseButtonPrefab, _PoseButtonParent);
        newBtn.onClick.AddListener(() => UKI_PoseManager.Instance.LoadPose(index));
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
            _LeftActuators[i]._NormExtension = _RightActuators[i]._NormExtension;
        }

        SetActuatorSliders();
    }

    public void MirrorLeft()
    {
        for (int i = 0; i < _LeftActuators.Count; i++)
        {
            _RightActuators[i]._NormExtension = _LeftActuators[i]._NormExtension;
        }

        SetActuatorSliders();
    }

    void SetActuatorExtension(ActuatorSlider actuatorSlider)
    {
        actuatorSlider._Actuator._NormExtension = actuatorSlider._Slider.value;
    }

    void CalibrateActuators()
    {
        foreach (TestActuator actuator in FindObjectsOfType<TestActuator>())
        {
            actuator.Calibrate();
        }
    }
}
