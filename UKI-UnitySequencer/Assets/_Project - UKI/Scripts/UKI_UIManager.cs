using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UKI_UIManager : MonoBehaviour
{
    // CAMERA
    public Transform _CamParent;
    [Range(0, 1)] float _CamZNorm = 0;
    public Vector2 _CamZRange;
    public Transform _Camera;
    float _CamZPos;
    [Range(0,1)] float _CamYRotNorm = 0;

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

    // Start is called before the first frame update
    void Start()
    {
        _SendToModBusToggle.isOn = UkiCommunicationsManager.Instance._SendToModbus;

        _Slider_CamRot.onValueChanged.AddListener(delegate { SetCamYRot(); });
        _Slider_CamZ.onValueChanged.AddListener(delegate { SetCamZDist(); });
        _EStopButton.onClick.AddListener(() => UkiCommunicationsManager.Instance.EStopButtonToggle());
        _SendToModBusToggle.onValueChanged.AddListener(delegate { UkiCommunicationsManager.Instance.SendToModbusToggle(_SendToModBusToggle); });

        _CalibrateButton.onClick.AddListener(CalibrateActuators);
    }

    // Update is called once per frame
    void Update()
    {
        _CamParent.SetLocalRotY(_CamYRotNorm * -360);
        _Camera.SetLocalZ(_CamZNorm.ScaleFrom01(_CamZRange.x, _CamZRange.y));
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

    void SetCamYRot()
    {
        _CamYRotNorm = _Slider_CamRot.value;
    }

    void SetCamZDist()
    {
        _CamZNorm = _Slider_CamZ.value;
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
