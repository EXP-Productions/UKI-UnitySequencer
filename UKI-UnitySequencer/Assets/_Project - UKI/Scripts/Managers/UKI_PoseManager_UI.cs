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
    public Button _ClearSequenceButton;

    public Toggle _MaskWingsToggle;

    public RectTransform _PoseLibraryContentParent;


    public Toggle _IgnoreCollisionToggle;
    public TMPro.TextMeshProUGUI _DEBUG_SequenceText;
  

    [Header("UI - SAVE POSE DIALOGUE")]
    public GameObject _SavePoseDialog;
    public Button _SavePoseButton, _MirrorLRButton, _MirrorRLButton, _ResetPoseButton;

    public InputField _SavePoseNameInput;

    string _ActiveButtonName = "";

    public ReorderableList _PoseLibraryList;
    public ReorderableList _PoseSequenceList;

    public RectTransform _SequencerButtonParent;

    public Slider _PlaybackSlider;
    public TMPro.TextMeshProUGUI _PlaybackStatusText;

    public GameObject _SaveSeqDialog;

    [Header("Prefabs")]
    public Button _SequencerPoseButtonPrefab;

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

        _ClearSequenceButton.onClick.AddListener(() => ClearActiveSequenceButtons());

        _ResetPoseButton.onClick.AddListener(UKI_PoseManager.Instance.CalibrationPose);

        _MirrorLRButton.onClick.AddListener(UKI_UIManager.Instance.MirrorLeftToRight);
        _MirrorRLButton.onClick.AddListener(UKI_UIManager.Instance.MirrorRightToLeft);

        _IgnoreCollisionToggle.onValueChanged.AddListener(delegate { SetIgnoreCollision();  } );

        _PlaybackSlider.onValueChanged.AddListener(call => UKI_PoseManager.Instance.ScrubSequence(_PlaybackSlider.value));
    }

    void SetIgnoreCollision()
    {
        UKI_PoseManager.Instance._IgnoreCollission = _IgnoreCollisionToggle.isOn;
        Debug.Log("Ignore collisions: " + UKI_PoseManager.Instance._IgnoreCollission);
    }

    private void Update()
    {
        // Highlight sequence button
        HighlightPoseSequenceButton();

        _DEBUG_SequenceText.text = "POSE MANAGER - Setting pose index: " + UKI_PoseManager.Instance._PoseSequenceIndex + "   ready count: " + UKI_PoseManager.Instance._ReadyCount + " / " + UKI_UIManager.Instance._AllActuators.Count;
    }

    public void AddPoseButtonToLibrary(string name)
    {
        Debug.Log("POSE UI - Button added: " + name);
        _PoseButtons.Add(CreatePoseButton(name, _PoseLibraryContentParent));
    }

    Button CreatePoseButton(string name, Transform parent)
    {
        // Instatiate
        Button newBtn = Instantiate(_SequencerPoseButtonPrefab, parent);// _PoseLibraryContentParent);
        // Set name
        newBtn.GetComponentInChildren<Text>().text = name;
        newBtn.name = "Pose button - " + name;
        // Add listener to set pose
        newBtn.onClick.AddListener(() => UKI_PoseManager.Instance.SetPoseByName(name, UKI_PoseManager.Instance._MaskWings));
        // Add listener to set active button name
        newBtn.onClick.AddListener(() => _ActiveButtonName = name);

        print("Created button pose sequence button: " + newBtn.name);

        return newBtn;
    }

    public void SelectSequence(SequenceData seqData)
    {
        // Clear the active seq buttons
        ClearActiveSequenceButtons();

        for (int i = 0; i < seqData._SequenceData.Count; i++)
        {
            // Get pose name
            string poseName = seqData._SequenceData[i];            

            Button newBtn = CreatePoseButton(poseName, _SequencerButtonParent);
            newBtn.gameObject.AddComponent<UI_RightClickDestroy>();

            /*
            // Create new button, set name, add set pose by name listener, add active button name listener, add right click destroy context menu
            Button newSequencePoseButton = Instantiate(_SequencerPoseButtonPrefab, _SequencerButtonParent);
            newSequencePoseButton.GetComponentInChildren<Text>().text = poseName;
            newSequencePoseButton.onClick.AddListener(() => UKI_PoseManager.Instance.SetPoseByName(poseName, UKI_PoseManager.Instance._MaskWings));
            newSequencePoseButton.onClick.AddListener(() => _ActiveButtonName = poseName);
            newSequencePoseButton.gameObject.AddComponent<UI_RightClickDestroy>();
            */

            // Add name to pose sequence
            UKI_PoseManager.Instance._ActiveSequencePoseList.Add(poseName);

            Debug.Log("POSE name - Button added: " + poseName);
        }
    }

    public void SetSequencePlayheadSlider(float norm)
    {
        _PlaybackSlider.SetValueWithoutNotify(norm);
    }

    void ClearActiveSequenceButtons()
    {
        if (_PoseSequenceList.Content.childCount <= 0)
            return;

        print("Clearing sequence buttons");

        int tempCount = _PoseSequenceList.Content.childCount;
        for (int i = 0; i < tempCount; i++)
        {
            Destroy(_PoseSequenceList.Content.GetChild(i).gameObject);
        }

        // Clear the active sequence
        UKI_PoseManager.Instance.ClearActiveSequencePoseList();
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
        AddPoseButtonToLibrary( UKI_PoseManager.Instance._PoseLibrary[UKI_PoseManager.Instance._PoseLibrary.Count - 1]._Name);
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

        print("Sequence list updated");

        Invoke("UpdateActiveSequenceListButtons", .1f);
    }

    public void UpdateActiveSequenceListButtons()
    {
        // Add poses based on names
        int listItemCount = _PoseSequenceList.Content.childCount;
        UKI_PoseManager.Instance._ActiveSequencePoseList.Clear();

        print("Updating sequence buttons with buttons count: " + listItemCount);
        for (int i = 0; i < listItemCount; i++)
        {
            // Get button in the pose sequence
            GameObject buttonObject = _PoseSequenceList.Content.GetChild(i).gameObject;
            
            // Get pose name
            string poseName = buttonObject.GetComponentInChildren<UnityEngine.UI.Text>().text;

            // Add name to pose sequence
            UKI_PoseManager.Instance._ActiveSequencePoseList.Add(poseName);

            UnityEngine.UI.Button button = buttonObject.GetComponent<Button>();
            button.onClick.RemoveAllListeners();

            int index = i;
            button.onClick.AddListener(() => UKI_PoseManager.Instance.SetPoseFromSequence(index));

            print("Pose button added to active sequence: " + index + "   " + poseName);            
        }

        UKI_PoseManager.Instance._PoseSequenceIndex = 0;
        HighlightPoseSequenceButton();
    }
    

    public void HighlightPoseSequenceButton()
    {
        int childCount = _PoseSequenceList.ContentLayout.transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            int index = i;
            ButtonHighlight btnHighlight = _PoseSequenceList.ContentLayout.transform.GetChild(index).GetComponentInChildren<ButtonHighlight>();
            if(btnHighlight != null)
                btnHighlight.Highlight(i == UKI_PoseManager.Instance._PoseSequenceIndex);
        }    
    }
}
