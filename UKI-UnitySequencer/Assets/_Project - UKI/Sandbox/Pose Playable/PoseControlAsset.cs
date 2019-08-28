using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PoseControlAsset : PlayableAsset
{
    public PoseData _PoseData;
    public float _NormExtention = 0f;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<ActuatorControlBehavior>.Create(graph);

        var actuatorBehavior = playable.GetBehaviour();
        actuatorBehavior._NormExtention = _NormExtention;

        return playable;
    }
}
