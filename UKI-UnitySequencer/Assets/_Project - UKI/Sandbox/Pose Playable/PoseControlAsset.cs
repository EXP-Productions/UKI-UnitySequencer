using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PoseControlAsset : PlayableAsset
{
    public PoseControlBehavior template;

    //public PoseData _PoseData;
    //public float _NormExtention = 0f;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<PoseControlBehavior>.Create(graph, template);
        return playable;
    }
}
