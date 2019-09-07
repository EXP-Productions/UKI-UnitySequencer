using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uki.Example.Animations;
using Unity.BlinkyBlinky;

public class XWash : IBlinkyAnimation
{
    float _Offset;
    float _Width = .3f;
    public Color _Col1 = Color.blue;
    public Color _Col2 = Color.yellow;
    
    public void Run()
    {
        _Offset = (Time.time / 3f)%1;
        float height = 6;
        
        // Iterate all LEDS to generate the next frame
        foreach (var pixel in BlinkyBlinky.pixels)
        {
            if (pixel._DistFromInputT > (_Offset * height) && pixel._DistFromInputT < (_Offset * height) + _Width)
                pixel.Color = Color.white * 255;
            else
                pixel.Color = Color.blue * 255;
        }
    }
}

public class DistFromCenterWash : IBlinkyAnimation
{
    public void Run()
    {

    }
}

