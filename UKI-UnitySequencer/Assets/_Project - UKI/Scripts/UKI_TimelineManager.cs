using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEngine.UI;


/// <summary>
/// TODO: Add in proxy legs for real world positions from actuators
/// </summary>
public class UKI_TimelineManager : MonoBehaviour
{
    enum State
    {
        Paused,             // Not moving but in what ever position it is in.
        Zeroed,             // All actuators zeroed out at base postion
        Zeroing,            // Transitioning to zeroed
        PlayingTimeline,    // Playing back timeline asset
    }

    public PlayableDirector _PlayableDirector;
    public TimelineAsset[] _Timeline;

    [Header("Limbs")]
    Leg[] _Legs;
    Wing[] _Wing;
    Pincer[] _Pincers;
    Abdomen _Abdomen;
    UKILimb[] _AllLimbs;

    [Header("UI")]
    public Button _Button_Play1;
    public Button _Button_Play2;
    public Button _Button_Play;
    public Button _Button_Pause;
    public Button _Button_Calibrate;

    // Start is called before the first frame update
    void Start()
    {
        // Find all the UKI limbs
        _Legs = FindObjectsOfType<Leg>();
        _Wing = FindObjectsOfType<Wing>();
        _Pincers = FindObjectsOfType<Pincer>();
        _Abdomen = FindObjectOfType<Abdomen>();

        _AllLimbs = FindObjectsOfType<UKILimb>();

        // UI
        _Button_Play1.onClick.AddListener(() => PlayTimeline(0, true));
        _Button_Play2.onClick.AddListener(() => PlayTimeline(1, true));

        _Button_Play.onClick.AddListener(() => Play());
        _Button_Pause.onClick.AddListener(() => Pause());
        _Button_Calibrate.onClick.AddListener(() => CalibrateToZero());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayTimeline(0, false);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) PlayTimeline(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) PlayTimeline(2);
        else if (Input.GetKeyDown(KeyCode.Z)) CalibrateToZero();
    }

    void Play()
    {
        _PlayableDirector.Play();

        for (int i = 0; i < _AllLimbs.Length; i++)
        {
            _AllLimbs[i].SetState(UKIEnums.State.Animating);
        }
    }

    void Pause()
    {
        _PlayableDirector.Pause();

        for (int i = 0; i < _AllLimbs.Length; i++)
        {
            _AllLimbs[i].SetState(UKIEnums.State.Paused);
        }
    }

    void PlayTimeline(int index, bool requireCalibration = true)
    {
        print("Attempting to play timeline " + index);
        if (requireCalibration && AllLimbsCalibrated())
        {
            _PlayableDirector.Stop();
            _PlayableDirector.Play(_Timeline[index]);

            for (int i = 0; i < _AllLimbs.Length; i++)
            {
                _AllLimbs[i].SetState(UKIEnums.State.Animating);
            }
            print("Timeline playing " + index);
        }
        else if(!requireCalibration)
        {
            _PlayableDirector.Stop();
            _PlayableDirector.Play(_Timeline[index]);

            for (int i = 0; i < _AllLimbs.Length; i++)
            {
                _AllLimbs[i].SetState(UKIEnums.State.Animating);
            }
            print("Timeline playing " + index);           
        }
        else
        {
            Debug.LogError("Wait for limbs to calibrate before running timeline");
        }
    }

    bool AllLimbsCalibrated()
    {
        int calibratedCount = 0;
        for (int i = 0; i < _AllLimbs.Length; i++)
        {
            if(_AllLimbs[i]._CalibratedToZero)
            {
                calibratedCount++;
            }
        }
        if(calibratedCount >= _AllLimbs.Length)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void CalibrateToZero()
    {
        // Stop the director
        _PlayableDirector.Stop();

        print("Calibrating");

        for (int i = 0; i < _AllLimbs.Length; i++)
        {
            _AllLimbs[i].CalibrateAllToZero();
        }
    }
    
    void LerpTimelines(float lerp)
    {
        
    }
}
