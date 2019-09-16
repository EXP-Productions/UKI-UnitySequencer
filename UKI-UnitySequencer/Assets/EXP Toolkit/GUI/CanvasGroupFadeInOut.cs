using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFadeInOut : MonoBehaviour
{
    CanvasGroup _CanvasGroup;
    public bool _FadeInOnEnable = false;
    public bool _DisableAfterFadeOut = false;
    public float _Duration = .5f;

    Action _FadeinCallback;
    Action _FadeoutCallback;

    private void Awake()
    {
        if (_CanvasGroup == null)
            _CanvasGroup = GetComponent<CanvasGroup>();

        if (!gameObject.activeSelf) _CanvasGroup.alpha = 0;
    }

    private void OnEnable()
    {
        if (_FadeInOnEnable)
            FadeIn(_Duration);
    }

    public void FadeIn()
    {
        StopAllCoroutines();
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        FadeIn(_Duration);
    }

    public void FadeIn(Action fadeInCompleteCallback)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        StopAllCoroutines();
        _FadeinCallback = fadeInCompleteCallback;
        FadeIn(_Duration);
    }

    public void FadeIn(float duration)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        if (_CanvasGroup == null)
            _CanvasGroup = GetComponent<CanvasGroup>();

        if (gameObject.activeSelf)
            StartCoroutine(FadeInRoutine(duration));
        else
            print("Not active can't fade");
    }

    IEnumerator FadeInRoutine(float duration)
    {
        float norm = 0;
        float timer = 0;

        while(norm < 1)
        {           
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            norm = timer / duration;

            _CanvasGroup.alpha = norm;
        }

        _CanvasGroup.alpha = 1;

        if (_FadeinCallback != null)
        {
            _FadeinCallback.Invoke();
            _FadeinCallback = null;
        }

        _CanvasGroup.interactable = true;
    }

    public void FadeOut()
    {
        if (gameObject.activeSelf)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutRoutine(_Duration));
        }
    }

    public void FadeOut(Action callback)
    {
        StopAllCoroutines();
        _FadeoutCallback = callback;
        StartCoroutine(FadeOutRoutine(_Duration));
    }

    public void FadeOut(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutRoutine(duration));
    }

    IEnumerator FadeOutRoutine(float duration)
    {
        float norm = 0;
        float timer = 0;

        while (norm < 1)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            norm = timer / duration;

            _CanvasGroup.alpha = 1-norm;
        }

        _CanvasGroup.alpha = 0;

        if (_FadeoutCallback != null)
        {
            _FadeoutCallback.Invoke();
            _FadeoutCallback = null;
        }

        _CanvasGroup.interactable = false;

        if (_DisableAfterFadeOut)
            gameObject.SetActive(false);
    }
}
