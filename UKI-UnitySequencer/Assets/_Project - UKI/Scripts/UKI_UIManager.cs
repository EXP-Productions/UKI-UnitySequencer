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


   
  
    // UI
    [HideInInspector]
    public ActuatorSlider[] _ActuatorSliders;

    [Header("UI")]
    public Button _EStopButton;
    public Button _CalibrateButton;
    public Toggle _SendToModBusToggle;
    public GameObject _EstopWarning;
    public Image _HeartBeatDisplay;

    public Button _MirrorLeftButton;
    public Button _MirrorRightButton;

    public Button _SaveCurrentPose;
    public Button _DeleteSelectedPose;
    List<Button> _PoseButtons = new List<Button>();
    public Toggle _LoopPosesToggle;
    public Toggle _MaskWingsToggle;

    public Slider _OfflineSpeedScalerSlider;



    public RectTransform _PoseButtonParent;
    public Button _SelectPoseButtonPrefab;

    public Toggle _OfflineSimModeToggle;

    public GameObject _SavePoseDialog;
    public Button _SavePoseNameButton;
    public InputField _SavePoseNameInput;

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

        _SaveCurrentPose.onClick.AddListener(() => UKI_PoseManager.Instance.SavePose());
        _DeleteSelectedPose.onClick.AddListener(() => UKI_PoseManager.Instance.DeletePose());

        _ActuatorSliders = FindObjectsOfType<ActuatorSlider>();

        _LoopPosesToggle.onValueChanged.AddListener((bool b) => UKI_PoseManager.Instance._LoopPoses = b);
        _MaskWingsToggle.onValueChanged.AddListener((bool b) => UKI_PoseManager.Instance._MaskWings = b);

        _OfflineSpeedScalerSlider.onValueChanged.AddListener((float f) => SetOfflineSpeedScaler(f));

        _SavePoseNameButton.onClick.AddListener(() => SavePose());

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

    public void SetOfflineSpeedScaler(float f)
    {
        print("Offline speed scaler: " + f);

        foreach (TestActuator act in _AllActuators)
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

    public void AddPoseButton(int index)
    {
        Button newBtn = Instantiate(_SelectPoseButtonPrefab, _PoseButtonParent);
        newBtn.GetComponentInChildren<Text>().text = UKI_PoseManager.Instance._AllPoses[index]._Name;
        newBtn.onClick.AddListener(() => UKI_PoseManager.Instance.SetPose(index, UKI_PoseManager.Instance._MaskWings));

        _PoseButtons.Add(newBtn);
    }

    public void RemovePoseButton()
    {
        Button btn = _PoseButtons[0];
        _PoseButtons.Remove(btn);
        Destroy(btn.gameObject);

        for (int i = 0; i < _PoseButtons.Count; i++)
        {
            _PoseButtons[i].onClick.RemoveAllListeners();
            int index = i;
            _PoseButtons[i].onClick.AddListener(() => UKI_PoseManager.Instance.SetPose(index));
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

    void SavePose()
    {
        PoseData newPoseData = new PoseData(UKI_UIManager.Instance._AllActuators, _SavePoseNameInput.text);
        UKI_PoseManager.Instance._AllPoses.Add(newPoseData);
        UKI_UIManager.Instance.AddPoseButton(UKI_PoseManager.Instance._AllPoses.Count - 1);
        JsonSerialisationHelper.Save(System.IO.Path.Combine(Application.streamingAssetsPath, "UKIPoseData.json"), UKI_PoseManager.Instance._AllPoses);
        print("Poses saved: " + UKI_PoseManager.Instance._AllPoses.Count);
        _SavePoseDialog.SetActive(false);
    }

}
