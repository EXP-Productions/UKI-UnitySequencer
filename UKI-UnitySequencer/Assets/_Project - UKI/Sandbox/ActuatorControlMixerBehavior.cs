using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ActuatorControlMixerBehavior : PlayableBehaviour
{   
    public float _NormExtention = 1f;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        Actuator actuator = playerData as Actuator;
        float finalNorm = 0f;

        if (!actuator)
        {
            Debug.Log("here");
            return;
        }

        // Get the number of all clips on this track
        int inputCount = playable.GetInputCount();

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<ActuatorControlBehavior> inputPlayable = (ScriptPlayable<ActuatorControlBehavior>)playable.GetInput(i);
            ActuatorControlBehavior input = inputPlayable.GetBehaviour();

            // Use the above variables to process each frame of this playable.
            finalNorm += input._NormExtention * inputWeight;
        }

        //assign the result to the bound object
        actuator._NormExtension = finalNorm;
    }
}
