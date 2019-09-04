using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uki.Example.Animations;
using Unity.BlinkyBlinky;

public class XWash : IBlinkyAnimation
{
    float _XPos;
    float _Width = .03f;
    public Color _Col1 = Color.blue;
    public Color _Col2 = Color.yellow;
    
    public void Run()
    {
        _XPos += Time.deltaTime;

        if (_XPos > 3)
            _XPos -= _XPos;

        // Iterate all LEDS to generate the next frame
        foreach (var pixel in BlinkyBlinky.pixels)
        {
            float x = Mathf.Abs(pixel.X) + _XPos;
            float lerp = x % 1;
            pixel.color = Color.Lerp(_Col1, _Col2, lerp);
        }
    }
}

public class DistFromCenterWash : IBlinkyAnimation
{
    public void Run()
    {

    }
}

