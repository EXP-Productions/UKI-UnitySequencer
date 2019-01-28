using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;


/// <summary>
/// TODO: Add in proxy legs for real world positions from actuators
/// </summary>
public class UKI_TimelineManager : MonoBehaviour
{
    enum State
    {
        Idle,               // Not moving but in what ever position it is in.
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

    // Start is called before the first frame update
    void Start()
    {
        // Find all the UKI limbs
        _Legs = FindObjectsOfType<Leg>();
        _Wing = FindObjectsOfType<Wing>();
        _Pincers = FindObjectsOfType<Pincer>();
        _Abdomen = FindObjectOfType<Abdomen>();

        _AllLimbs = FindObjectsOfType<UKILimb>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayTimeline(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) PlayTimeline(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) PlayTimeline(2);
        else if (Input.GetKeyDown(KeyCode.Z)) CalibrateToZero();
    }

    void PlayTimeline(int index)
    {
        if (AllLimbsCalibrated())
        {
            _PlayableDirector.Stop();
            _PlayableDirector.Play(_Timeline[index]);

            for (int i = 0; i < _AllLimbs.Length; i++)
            {
                _AllLimbs[i].SetState(UKILimb.State.Animating);
            }
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

        for (int i = 0; i < _AllLimbs.Length; i++)
        {
            _AllLimbs[i].CalibrateAllToZero();
        }
    }
    
    void LerpTimelines(float lerp)
    {
        
    }
}
