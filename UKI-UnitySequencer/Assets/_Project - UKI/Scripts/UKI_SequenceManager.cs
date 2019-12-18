using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.UI.Extensions;

[System.Serializable]
public class SequenceData
{
    public string _Name;
    public List<string> _SequenceData = new List<string>();

    public SequenceData()
    {
    }

    public SequenceData(List<string> poses, string name)
    {
        _Name = name;
        _SequenceData = poses;
    }
}

public class UKI_SequenceManager : MonoBehaviour
{
    public static UKI_SequenceManager Instance;

    public List<SequenceData> _SeqLibrary = new List<SequenceData>();

    [Header("UI - SEQ MANAGER")]
    public Button _OpenSaveDialogueButton;
    public Button _DeleteSelectedSequence;
    List<Button> _SequenceButtons = new List<Button>();

    public RectTransform _SequenceButtonParent;
    public Button _SelectSequenceButtonPrefab;

    string _ActiveButtonName = "";

    [Header("UI - SAVE Sequence DIALOGUE")]
    public GameObject _SaveDialog;
    public Button _SaveButton;
    public InputField _SaveNameInput;

    public ReorderableList _LibraryList;

    string _FileName = "UKISeqData.json";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _OpenSaveDialogueButton.onClick.AddListener(() => OpenSaveDialogue());
        _DeleteSelectedSequence.onClick.AddListener(() => RemoveActiveButton());

        _SaveButton.onClick.AddListener(() => SaveSequence());

        LoadAll();
    }


    void LoadAll()
    {
        _SeqLibrary = JsonSerialisationHelper.LoadFromFile<List<SequenceData>>(Path.Combine(Application.streamingAssetsPath, _FileName)) as List<SequenceData>;

        for (int i = 0; i < _SeqLibrary.Count; i++)
            AddSequenceButton(_SeqLibrary[i]._Name);

        print(name + " Sequences loaded: " + _SeqLibrary.Count);
    }

    public void AddSequenceButton(string name)
    {
        Debug.Log("Sequence UI - Button added: " + name);

        Button newBtn = Instantiate(_SelectSequenceButtonPrefab, _SequenceButtonParent);
        newBtn.GetComponentInChildren<Text>().text = name;
        newBtn.onClick.AddListener(() => SetSeqByName(name));
        newBtn.onClick.AddListener(() => _ActiveButtonName = name);

        _SequenceButtons.Add(newBtn);
    }
    
    public void SetSeqByName(string name)
    {
        print("Setting pose by name: " + name);

        SequenceData seqData = _SeqLibrary.Single(s => s._Name == name);
        UKI_PoseManager_UI.Instance.SelectSequence(seqData);
    }
    
    public void RemoveActiveButton()
    {
        Button activeButton = _SequenceButtons.Single(s => s.GetComponentInChildren<Text>().text == _ActiveButtonName);

        if (activeButton == null)
        {
            print("No active sequence button to remove");
            return;
        }

        print("Removing active sequence button: " + _ActiveButtonName);
        _SequenceButtons.Remove(activeButton);
        Destroy(activeButton.gameObject);

        DeleteSequence(_ActiveButtonName);

        _ActiveButtonName = "";
    }

    public void OpenSaveDialogue()
    {
        _SaveDialog.SetActive(true);
    }

    void SaveSequence()
    {
        if (_SaveNameInput.text == "")
        {
            print("Can't save with blank name");
            return;
        }

        print("Saving sequence: " + _SaveNameInput.text);
        for (int i = 0; i < UKI_PoseManager.Instance._PoseSequence.Count; i++)
        {
            print(UKI_PoseManager.Instance._PoseSequence[i]);
        }

        SequenceData newSeqData = new SequenceData(UKI_PoseManager.Instance._PoseSequence, _SaveNameInput.text);
        _SeqLibrary.Add(newSeqData);
        AddSequenceButton(_SeqLibrary[_SeqLibrary.Count - 1]._Name);
        JsonSerialisationHelper.Save(System.IO.Path.Combine(Application.streamingAssetsPath, _FileName), _SeqLibrary);
        print("Sequence saved: " + _SeqLibrary.Count);
        _SaveDialog.SetActive(false);
    }

    public void DeleteSequence(string name)
    {
        SequenceData data = _SeqLibrary.Single(p => p._Name == name);

        if (data != null)
        {
            _SeqLibrary.Remove(data);
            JsonSerialisationHelper.Save(Path.Combine(Application.streamingAssetsPath, _FileName), _SeqLibrary);
            print("Sequence deleted: " + _SeqLibrary.Count);
        }
        else
        {
            print("Cannot find sequence to remove: " + _SeqLibrary.Count);
        }
    }
}
