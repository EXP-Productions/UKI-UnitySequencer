using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

public class ContextMenuGUI : MonoBehaviour
{
    public static ContextMenuGUI Instance;

    List<Button> _Buttons = new List<Button>();
    public Button _ButtonPrefab;

    public UnityEvent<int> _SelectionEvent;

    Action<int> _Callback;

    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            Close();
        }
    }

    public void Open(string[] selections, Action<int> callback)
    {
        gameObject.SetActive(true);

        _Callback = callback;

        transform.localPosition = new Vector3(Input.mousePosition.x - (Screen.width*.5f), Input.mousePosition.y - (Screen.height * .5f), 0);

        for (int i = 0; i < selections.Length; i++)
        {
            if (i < _Buttons.Count)
            {
                _Buttons[i].gameObject.SetActive(true);
                _Buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = selections[i];
            }
            else
            {
                Button newBtn = Instantiate(_ButtonPrefab);
                newBtn.transform.SetParent(transform);
                newBtn.GetComponentInChildren<TextMeshProUGUI>().text = selections[i];

                int index = i;
                newBtn.onClick.AddListener(() => Select(index));

                _Buttons.Add(newBtn);
            }
        }
    }

    private void Select(int index)
    {
        _Callback.Invoke(index);
        Close();
    }

    void Close()
    {
        gameObject.SetActive(false);

        foreach (Button btn in _Buttons)
        {
            btn.gameObject.SetActive(false);
        }

        _Callback = null;
    }
}
