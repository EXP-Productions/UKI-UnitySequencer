using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class PoseControlMixerBehavior : PlayableBehaviour
{
    float prevNorm = 0;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Actuator actuator = playerData as Actuator;
        float finalNorm = 0f;

        if (!actuator)
        {
            return;
        }

        // Get the number of all clips on this track
        int inputCount = playable.GetInputCount();

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<PoseControlBehavior> inputPlayable = (ScriptPlayable<PoseControlBehavior>)playable.GetInput(i);
            PoseControlBehavior input = inputPlayable.GetBehaviour();
            
            // Use the above variables to process each frame of this playable.
            finalNorm += input._NormExtention * inputWeight;
        }


        float extensionPerSecond = (finalNorm - prevNorm) * (1 / info.deltaTime);

        // Check delta
        if (extensionPerSecond > actuator.MaxExtensionPersecond)
        {
            //Debug.LogWarning("Extention rate warning - Current / Max extension p/s: " + extensionPerSecond + " / " + actuator.MaxExtensionPersecond);
            //Debug.LogWarning("Full extension time: " + actuator._FullExtensionDuration);
        }
        
        prevNorm = finalNorm;

        //assign the result to the bound object
        actuator.TargetNormExtension = finalNorm;
    }
}
