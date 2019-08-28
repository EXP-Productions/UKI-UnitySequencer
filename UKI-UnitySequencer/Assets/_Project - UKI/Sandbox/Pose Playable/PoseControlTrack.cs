using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(PoseControlAsset))]
[TrackBindingType(typeof(UKI_PoseManager))]
public class PoseControlTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<PoseControlMixerBehavior>.Create(graph, inputCount);
    }
}
