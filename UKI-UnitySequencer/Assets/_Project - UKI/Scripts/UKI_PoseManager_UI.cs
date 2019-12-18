using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Linq;

public class UKI_PoseManager_UI : MonoBehaviour
{
    public static UKI_PoseManager_UI Instance;

    [Header("UI - POSE MANAGER")]
    public Button _OpenSaveDialogueButton;
    public Button _DeleteSelectedPose;
    List<Button> _PoseButtons = new List<Button>();

    public Button _PlayButton, _PauseButton, _StopButton;

    public Toggle _MaskWingsToggle;

    public RectTransform _PoseButtonParent;
    public Button _SelectPoseButtonPrefab;

    [Header("UI - SAVE POSE DIALOGUE")]
    public GameObject _SavePoseDialog;
    public Button _SavePoseButton;
    public InputField _SavePoseNameInput;

    string _ActiveButtonName = "";

    public ReorderableList _PoseLibraryList;
    public ReorderableList _PoseSequenceList;

    public RectTransform _SequencerButtonParent;

    public Slider _PlaybackSlider;

    public GameObject _SaveSeqDialog;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _OpenSaveDialogueButton.onClick.AddListener(() => OpenSaveDialogue());
        _DeleteSelectedPose.onClick.AddListener(() => RemoveActivePoseButton());

        _PlayButton.onClick.AddListener(() => UKI_PoseManager.Instance.SetState(SequencerState.Playing));
        _PauseButton.onClick.AddListener(() => UKI_PoseManager.Instance.SetState(SequencerState.Paused));
        _StopButton.onClick.AddListener(() => UKI_PoseManager.Instance.SetState(SequencerState.Stopped));

        _MaskWingsToggle.onValueChanged.AddListener((bool b) => UKI_PoseManager.Instance._MaskWings = b);
        _SavePoseButton.onClick.AddListener(() => SavePose());

        _PoseLibraryList.OnElementDropped.AddListener(call => HandleSequenceListUpdated(call));
        _PoseSequenceList.OnElementAdded.AddListener(call => HandleSequenceListUpdated(call));

        _PlaybackSlider.onValueChanged.AddListener(call => UKI_PoseManager.Instance.ScrubSequence(_PlaybackSlider.value));
    }

    private void Update()
    {
        HighlightSequenceButton();
    }

    public void AddPoseButton(string name)
    {
        Debug.Log("POSE UI - Button added: " + name);

        Button newBtn = Instantiate(_SelectPoseButtonPrefab, _PoseButtonParent);
        newBtn.GetComponentInChildren<Text>().text = name;
        newBtn.onClick.AddListener(() => UKI_PoseManager.Instance.SetPoseByName(name, UKI_PoseManager.Instance._MaskWings));
        newBtn.onClick.AddListener(() => _ActiveButtonName = name);

        _PoseButtons.Add(newBtn);
    }

    public void SelectSequence(SequenceData seqData)
    {
        Debug.Log("Setting up sequence: " + seqData._Name);

        ClearSequence();

        for (int i = 0; i < seqData._SequenceData.Count; i++)
        {
            string name = seqData._SequenceData[i];
            Debug.Log("POSE name - Button added: " + name);

            Button newBtn = Instantiate(_SelectPoseButtonPrefab, _SequencerButtonParent);
            newBtn.GetComponentInChildren<Text>().text = name;
            newBtn.onClick.AddListener(() => UKI_PoseManager.Instance.SetPoseByName(name, UKI_PoseManager.Instance._MaskWings));
            newBtn.onClick.AddListener(() => _ActiveButtonName = name);

            newBtn.gameObject.AddComponent<UI_RightClickDestroy>();
        }

        UpdateSequenceListButtons();
    }

    void ClearSequence()
    {
        int listItemCount = _PoseSequenceList.Content.childCount;
        for (int i = 0; i < listItemCount; i++)
        {
            Destroy(_PoseSequenceList.Content.GetChild(i).gameObject);
        }

        UpdateSequenceListButtons();
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
        UKI_PoseManager.Instance._PoseLibrary.Add(newPoseData);
        AddPoseButton( UKI_PoseManager.Instance._PoseLibrary[UKI_PoseManager.Instance._PoseLibrary.Count - 1]._Name);
        JsonSerialisationHelper.Save(System.IO.Path.Combine(Application.streamingAssetsPath, "UKIPoseData.json"), UKI_PoseManager.Instance._PoseLibrary);
        print("Poses saved: " + UKI_PoseManager.Instance._PoseLibrary.Count);
        _SavePoseDialog.SetActive(false);
    }

    public void HandleSequenceListUpdated(UnityEngine.UI.Extensions.ReorderableList.ReorderableListEventStruct item)
    {
        if (item.ToList != _PoseSequenceList)
            return;

        // Add right click destroy
        if (item.DroppedObject.GetComponent<UI_RightClickDestroy>() == null)
            item.DroppedObject.AddComponent<UI_RightClickDestroy>();

        Invoke("UpdateSequenceListButtons", .1f);
    }

    public void UpdateSequenceListButtonsAfterWait()
    {
        Invoke("UpdateSequenceListButtons", .1f);
    }

    public void UpdateSequenceListButtons()
    {
        // Clear pose sequence
        UKI_PoseManager.Instance._PoseSequence.Clear();
        print("Pose sequence...");

        // Add poses based on names
        int listItemCount = _PoseSequenceList.Content.childCount;
        for (int i = 0; i < listItemCount; i++)
        {
            GameObject buttonObject = _PoseSequenceList.Content.GetChild(i).gameObject;
          
            string poseName = buttonObject.GetComponentInChildren<UnityEngine.UI.Text>().text;
            UKI_PoseManager.Instance._PoseSequence.Add(poseName);

            UnityEngine.UI.Button button = buttonObject.GetComponent<Button>();
            button.onClick.RemoveAllListeners();

            int index = i;
            button.onClick.AddListener(() => UKI_PoseManager.Instance.SetPoseFromSequence(index));

            print("Pose: " + index + "   " + poseName);            
        }

        UKI_PoseManager.Instance._PoseSequenceIndex = 0;
        HighlightSequenceButton();
    }
    

    public void HighlightSequenceButton()
    {
       // if(_PoseSequenceList.Content.childCount > 0 && !_SavePoseDialog.activeSelf && !_SaveSeqDialog.activeSelf)
       //     _PoseSequenceList.Content.GetChild(UKI_PoseManager.Instance._PoseSequenceIndex).GetComponent<Button>().Select();        
    }
}
