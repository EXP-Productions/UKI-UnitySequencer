using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Associates a UI slider with an actuator
public class ActuatorSlider : MonoBehaviour
{
    public UkiActuatorAssignments _ActuatorAssignment;

    public Actuator _Actuator;

    Image _BackgroundImage;

    public TextMeshProUGUI _TextName;

    public Toggle _CollidedToggle;

    public Image _ReportedSliderImage;

    public Slider _Slider;

    Color _BGNormalCol = new Color(.2f, .2f, .2f);
    Color _BGAnimatingCol = new Color(.68f, .68f, .1f);

    public Color _NormalCol;
    public Color _CollidedCol;
    public Color _AnimatingCol = Color.green;

    ColorBlock _ColBlock;

    private void Start()
    {
        Image[] images = _Slider.GetComponentsInChildren<Image>();
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].name == "Background")
                _BackgroundImage = images[i];
        }
        _BackgroundImage.color = _BGNormalCol;

        Actuator[] actuators = FindObjectsOfType<Actuator>();
        for (int i = 0; i < actuators.Length; i++)
        {
            if (actuators[i]._ActuatorIndex == _ActuatorAssignment)
                _Actuator = actuators[i];
        }

        _ColBlock = new ColorBlock();
        _ColBlock.normalColor = Color.gray;
        _ColBlock.highlightedColor = Color.gray * 1.3f;
        _ColBlock.pressedColor = Color.gray * .65f;

        if (_Actuator == null)
        {
            print("UI INIT: " + name + " no actuator found");
        }
        else
        {
            _Slider.onValueChanged.AddListener((float f) => _Actuator.TargetNormExtension = f);
            _TextName.text = _ActuatorAssignment.ToString();
        }

        

        if (_Slider == null)
            print(name);
    }

    private void Update()
    {
        if (_Actuator != null)
        {
            _CollidedToggle.isOn = _Actuator._Collided;
        }
        else
            print(name);


        switch (_Actuator._State)
        {
            case UKIEnums.State.Animating:
                _BackgroundImage.color = _BGAnimatingCol;
                _ColBlock.normalColor = Color.yellow;
                break;
            case UKIEnums.State.Paused:
                _BackgroundImage.color = _BGNormalCol;
                _ColBlock.normalColor = Color.gray;
                break;
            case UKIEnums.State.NoiseMovement:
                _BackgroundImage.color = _BGAnimatingCol;
                _ColBlock.normalColor = Color.blue;
                break;
        }

        if (!_Actuator.IsAtTargetPosition())
        {
            _TextName.text = _ActuatorAssignment.ToString() + "  " + _Actuator._ReportedExtensionInMM.ToString("##") + "/" + _Actuator._CurrentTargetExtensionMM.ToString("##") + "   " + _Actuator._ReportedExtensionDiff.ToString("##");
            _ReportedSliderImage.rectTransform.anchorMax = new Vector2(_Actuator._ReportedNormExtension, 0);
        }
        else
        {
            _TextName.text = _ActuatorAssignment.ToString();
            _ReportedSliderImage.rectTransform.anchorMax = new Vector2(_Actuator.TargetNormExtension, 0);
        }



      

        //_Slider.colors = _ColBlock;
        //190 190 40
    }

    [ContextMenu("Name")]
    public void Name()
    {
        name = "Actuator Control - " + _ActuatorAssignment.ToString();
    }

    public void SetToActuatorNorm(bool setWithoutNotify)
    {
        if (_Actuator == null)
            print(name);


        if (setWithoutNotify)
            _Slider.SetValueWithoutNotify(_Actuator.TargetNormExtension);
        else
            _Slider.normalizedValue = _Actuator.TargetNormExtension;
    }
}
