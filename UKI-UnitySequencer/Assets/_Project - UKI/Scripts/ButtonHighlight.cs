using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonHighlight : MonoBehaviour
{
    Image _Image;

    void Awake()
    {
        _Image = GetComponent<Image>();
        Highlight(false);
    }

    // Update is called once per frame
    public void Highlight(bool highlight)
    {
        _Image.color = highlight ? Color.white : new Color(1, 1, 1, 0);
    }
}
