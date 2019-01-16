using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

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
    public Leg[] _Legs;
    public Wing[] _Wing;
    public Pincer[] _Pincers;
    public Abdomen _Abdomen;

    // Start is called before the first frame update
    void Start()
    {
        // Find all the UKI limbs
        _Legs = FindObjectsOfType<Leg>();
        _Wing = FindObjectsOfType<Wing>();
        _Pincers = FindObjectsOfType<Pincer>();
        _Abdomen = FindObjectOfType<Abdomen>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayTimeline(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) PlayTimeline(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) PlayTimeline(2);
        else if (Input.GetKeyDown(KeyCode.Z)) ReturnToZero();
    }

    void PlayTimeline(int index)
    {
        _PlayableDirector.Play(_Timeline[index]);
    }


    void ReturnToZero()
    {
        // Stop the director
        _PlayableDirector.Pause();

       
    }
    
    void LerpTimelines(float lerp)
    {
        
    }
}
