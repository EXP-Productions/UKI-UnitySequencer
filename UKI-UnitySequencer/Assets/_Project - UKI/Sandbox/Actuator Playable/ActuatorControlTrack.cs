using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(ActuatorControlAsset))]
[TrackBindingType(typeof(Actuator))]
public class ActuatorControlTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<ActuatorControlMixerBehavior>.Create(graph, inputCount);
    }
}
