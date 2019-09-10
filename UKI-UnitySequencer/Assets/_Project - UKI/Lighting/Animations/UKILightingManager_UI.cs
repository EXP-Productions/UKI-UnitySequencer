using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UKILightingManager_UI : MonoBehaviour
{
    public TMPro.TMP_Dropdown _SourceDropDown;
    UKILightingManager _LightingManager;

    // Start is called before the first frame update
    void Start()
    {
        _LightingManager = UKILightingManager.Instance;

        _SourceDropDown.AddOptions
        (
            new List<string>()
            {
                UKILightingManager.AnimationSource.NDI0.ToString(),
                UKILightingManager.AnimationSource.NDI1.ToString(),
                UKILightingManager.AnimationSource.Animation0.ToString(),
            }
        );

        _SourceDropDown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(_SourceDropDown);
        });
    }

    //Ouput the new value of the Dropdown into Text
    void DropdownValueChanged(TMPro.TMP_Dropdown change)
    {
        _LightingManager.SetAnimSource(change.value);
    }
}
