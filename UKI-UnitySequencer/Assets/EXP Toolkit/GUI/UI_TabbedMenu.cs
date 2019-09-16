using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MenuTab
{
    public Button _ActivationButton;
    public CanvasGroupFadeInOut _CanvasFade;
}

// A menu that uses tabs that are alays visible to select between pages
public class UI_TabbedMenu : MonoBehaviour
{
    int _SelectedIndex = 0;
    public MenuTab[] _MenuItems;
    public float _FadeDuration = .1f;

    private void Start()
    {
        for (int i = 0; i < _MenuItems.Length; i++)
        {
            int index = i;
            _MenuItems[i]._ActivationButton.onClick.AddListener(() => SelectMenuItem(index));

            if (i != 0)
            {
                _MenuItems[i]._CanvasFade.GetComponent<CanvasGroup>().alpha = 0;
                _MenuItems[i]._CanvasFade.gameObject.SetActive(false);
            }
            else
            {
                _MenuItems[i]._CanvasFade.GetComponent<CanvasGroup>().alpha = 1;
                _MenuItems[i]._CanvasFade.gameObject.SetActive(true);
            }

            _MenuItems[i]._CanvasFade._Duration = _FadeDuration;
        }
    }

    void SelectMenuItem(int index)
    {
        // RETURN IF THE SAME INDEX YOU ARE CURRENTLY VIEWING
        if (_SelectedIndex == index)
            return;

        // FADE OUT CURRENT STATE
        _MenuItems[_SelectedIndex]._CanvasFade.FadeOut();

        _SelectedIndex = index;

        // FADE IN NEW STATE
        _MenuItems[_SelectedIndex]._CanvasFade.FadeIn();
    }
}
