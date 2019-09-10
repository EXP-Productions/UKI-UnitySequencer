using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Linq;

public class UKI_PoseManager_UI : MonoBehaviour
{
    public static UKI_PoseManager_UI Instance;

    [Header("UI - POSE MANAGER")]
    public Button _OpenSaveDialogueButton;
    public Button _DeleteSelectedPose;
    List<Button> _PoseButtons = new List<Button>();
    public Toggle _LoopPosesToggle;
    public Toggle _MaskWingsToggle;

    public RectTransform _PoseButtonParent;
    public Button _SelectPoseButtonPrefab;

    [Header("UI - SAVE POSE DIALOGUE")]
    public GameObject _SavePoseDialog;
    public Button _SavePoseButton;
    public InputField _SavePoseNameInput;

    string _ActiveButtonName = "";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _OpenSaveDialogueButton.onClick.AddListener(() => OpenSaveDialogue());
        _DeleteSelectedPose.onClick.AddListener(() => RemoveActivePoseButton());

        _LoopPosesToggle.onValueChanged.AddListener((bool b) => UKI_PoseManager.Instance._LoopPoses = b);
        _MaskWingsToggle.onValueChanged.AddListener((bool b) => UKI_PoseManager.Instance._MaskWings = b);
        _SavePoseButton.onClick.AddListener(() => SavePose());
    }

    public void AddPoseButton(string name)
    {
        Button newBtn = Instantiate(_SelectPoseButtonPrefab, _PoseButtonParent);
        newBtn.GetComponentInChildren<Text>().text = name;
        newBtn.onClick.AddListener(() => UKI_PoseManager.Instance.SetPose(name, UKI_PoseManager.Instance._MaskWings));
        newBtn.onClick.AddListener(() => _ActiveButtonName = name);

        _PoseButtons.Add(newBtn);
    }

    public void RemoveActivePoseButton()
    {
        Button activeButton = _PoseButtons.Single(s => s.GetComponentInChildren<Text>().text == _ActiveButtonName);
        
        if(activeButton == null)
        {
            print("No active pose button to remove");
            return;
        }
       
        print("Removing active pose button: " + _ActiveButtonName);
        _PoseButtons.Remove(activeButton);
        Destroy(activeButton.gameObject);
               
        UKI_PoseManager.Instance.DeletePose(_ActiveButtonName);

        _ActiveButtonName = "";
    }

    public void OpenSaveDialogue()
    {
        _SavePoseDialog.SetActive(true);
    }

    void SavePose()
    {
        PoseData newPoseData = new PoseData(UKI_UIManager.Instance._AllActuators, _SavePoseNameInput.text);
        UKI_PoseManager.Instance._AllPoses.Add(newPoseData);
        AddPoseButton( UKI_PoseManager.Instance._AllPoses[UKI_PoseManager.Instance._AllPoses.Count - 1]._Name);
        JsonSerialisationHelper.Save(System.IO.Path.Combine(Application.streamingAssetsPath, "UKIPoseData.json"), UKI_PoseManager.Instance._AllPoses);
        print("Poses saved: " + UKI_PoseManager.Instance._AllPoses.Count);
        _SavePoseDialog.SetActive(false);
    }

}
