using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Linq;

[ExecuteInEditMode]
public class TimelineWrangler : MonoBehaviour
{
    public TimelineAsset _Timeline;
    public Actuator actuator;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        IEnumerable<TrackAsset> tracks = _Timeline.GetOutputTracks();

        double easeInDuration = 0;
        int clipIndex = 0;
        double prevEndTime = 0;
        
        foreach (TrackAsset track in _Timeline.GetOutputTracks())
        {
            foreach (TimelineClip clip in track.GetClips())
            {
                if (clipIndex > 0)
                {
                    easeInDuration = prevEndTime - clip.start;

                    if (easeInDuration < actuator._FullExtensionDuration)
                    {
                        print(track.name + "    " +  clip.displayName + " ease in duration too low. " + easeInDuration + " / " + actuator._FullExtensionDuration);
                    }
                }

                prevEndTime = clip.end;
                clipIndex++;
            }
        }
    }
}
