using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Klak.Ndi;

public class UKILightingManager_UI : MonoBehaviour
{
    public TMPro.TMP_Dropdown _SourceDropDown;
    UKILightingManager _LightingManager;

    public Button _RefreshButton;

    public List<string> _NDISourceNames = new List<string>();
    List<string> _AnimationNames = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        _LightingManager = UKILightingManager.Instance;

        _AnimationNames = new List<string>() { "Anim Distance Ring", "Anim Z Sweep" };

        _SourceDropDown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(_SourceDropDown);
        });

        _RefreshButton.onClick.AddListener(() => Refresh());
        Refresh();
    }

    //Ouput the new value of the Dropdown into Text
    void DropdownValueChanged(TMPro.TMP_Dropdown change)
    {
        if(change.options[change.value].text.Contains("Anim "))
        {
            _LightingManager.SetAnimSource(UKILightingManager.AnimationSource.Animation, change.options[change.value].text.Replace("Anim ", ""));
        }
        else
        {
            _LightingManager.SetAnimSource(UKILightingManager.AnimationSource.NDI, change.options[change.value].text);
        }
    }

    public void Refresh()
    {
        NdiManager.GetSourceNames(_NDISourceNames);

        for (int i = 0; i < _NDISourceNames.Count; i++)
        {
            Debug.Log("Source " + i + "  " + _NDISourceNames[i]);
        }

        _SourceDropDown.ClearOptions();
        _SourceDropDown.AddOptions(_NDISourceNames);
        _SourceDropDown.AddOptions(_AnimationNames);
    }
}
