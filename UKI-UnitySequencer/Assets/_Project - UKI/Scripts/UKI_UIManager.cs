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
    public Button _CalibrateButton;
    public Toggle _SendToModBusToggle;
    public GameObject _EstopWarning;
    public Image _HeartBeatDisplay;
    public Slider _OfflineSpeedScalerSlider;
    public Toggle _OfflineSimModeToggle;

    //public Button _MirrorLeftButton;
    //public Button _MirrorRightButton;

    [Header("UI - POSE MANAGER")]
    public Button _SaveCurrentPose;
    public Button _DeleteSelectedPose;
    List<Button> _PoseButtons = new List<Button>();
    public Toggle _LoopPosesToggle;
    public Toggle _MaskWingsToggle;

    public RectTransform _PoseButtonParent;
    public Button _SelectPoseButtonPrefab;

   

    public GameObject _SavePoseDialog;
    public Button _SavePoseNameButton;
    public InputField _SavePoseNameInput;

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
                    actuator._NormExtension += 0.1f;
                }
                else
                {
                    actuator._NormExtension -= 0.1f;
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
                    actuator._NormExtension = 1.0f;
                }
                else
                {
                    actuator._NormExtension = 0.0f;
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
                    actuator._NormExtension = 1.0f;
                }
                else
                {
                    actuator._NormExtension = 0.0f;
                }
            }
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        _SendToModBusToggle.isOn = UkiCommunicationsManager.Instance._SendToModbus;

        _EStopButton.onClick.AddListener(() => UkiCommunicationsManager.Instance.EStopButtonToggle());
        _SendToModBusToggle.onValueChanged.AddListener(delegate { UkiCommunicationsManager.Instance.SendToModbusToggle(_SendToModBusToggle); });
        _OfflineSimModeToggle.onValueChanged.AddListener((bool b) => ToggleOfflineSimMode(b));

       // _MirrorLeftButton.onClick.AddListener(() => MirrorLeft());
       // _MirrorRightButton.onClick.AddListener(() => MirrorRight());

        _CalibrateButton.onClick.AddListener(CalibrateActuators);

        _SaveCurrentPose.onClick.AddListener(() => UKI_PoseManager.Instance.SavePose());
        _DeleteSelectedPose.onClick.AddListener(() => UKI_PoseManager.Instance.DeletePose());

        _ActuatorSliders = FindObjectsOfType<ActuatorSlider>();

        _LoopPosesToggle.onValueChanged.AddListener((bool b) => UKI_PoseManager.Instance._LoopPoses = b);
        _MaskWingsToggle.onValueChanged.AddListener((bool b) => UKI_PoseManager.Instance._MaskWings = b);

        _OfflineSpeedScalerSlider.onValueChanged.AddListener((float f) => SetOfflineSpeedScaler(f));

        _SavePoseNameButton.onClick.AddListener(() => SavePose());

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
        foreach (Actuator actuator in FindObjectsOfType<Actuator>())
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
