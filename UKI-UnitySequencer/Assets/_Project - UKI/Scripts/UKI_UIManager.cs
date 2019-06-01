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

    public Slider _Slider_CamRot;
    public Slider _Slider_CamZ;
    [HideInInspector]
    public ActuatorSlider[] _ActuatorSliders;

    public Button _EStopButton;
    public Button _CalibrateButton;

    // Start is called before the first frame update
    void Start()
    {
        _Slider_CamRot.onValueChanged.AddListener(delegate { SetCamYRot(); });
        _Slider_CamZ.onValueChanged.AddListener(delegate { SetCamZDist(); });
        _EStopButton.onClick.AddListener(() => UkiCommunicationsManager.Instance.EStop());
        _CalibrateButton.onClick.AddListener(CalibrateActuators);
    }

    // Update is called once per frame
    void Update()
    {
        _CamParent.SetLocalRotY(_CamYRotNorm * -360);
        _Camera.SetLocalZ(_CamZNorm.ScaleFrom01(_CamZRange.x, _CamZRange.y));
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
