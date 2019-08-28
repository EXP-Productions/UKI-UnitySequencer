using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class PoseControlBehavior : PlayableBehaviour
{
    public float _NormExtention = 1f;

    float _PrevNorm = 0;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Actuator actuator = playerData as Actuator;        

        if (actuator != null)
        {
            actuator.NormExtension = _NormExtention;

            float extensionPerSecond = (Mathf.Abs(_NormExtention - _PrevNorm)) * (1 / info.deltaTime);

            if (extensionPerSecond > actuator.MaxExtensionPersecond)
            {
                Debug.Log("Extending too quickly: " + extensionPerSecond + " / Maximum: " + actuator.MaxExtensionPersecond);
                Debug.Log("Extend by: " + (extensionPerSecond / actuator.MaxExtensionPersecond) );
            }

            _PrevNorm = _NormExtention;
        }
    }
}
