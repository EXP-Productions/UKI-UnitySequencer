﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.BlinkyBlinky;
using UnityEngine.Profiling;

public class RenderTextureMapper : MonoBehaviour
{
    public RenderTexture _RTex;
    public Vector2 _SampleUV;
    public int _SampleCount = 3000;
    Color[] _Cols;

    public Transform _UVQuad;
    List<Vector3> _LEDUVs = new List<Vector3>();

    RenderTexture _RTexTemp;
    Texture2D _Tex2D;

    // Start is called before the first frame update
    void Start()
    {
        _Cols = new Color[_SampleCount];


        _RTexTemp = new RenderTexture(
                       _RTex.width,
                       _RTex.height,
                       0,
                       RenderTextureFormat.Default,
                       RenderTextureReadWrite.Linear);

        // Create a new readable Texture2D to copy the pixels to it
        _Tex2D = new Texture2D(_RTex.width, _RTex.height);

    }

    // Update is called once per frame
    public void ManualUpdate()
    {
        // Blit the pixels on texture to the RenderTexture
        Graphics.Blit(_RTex, _RTexTemp);

        // Backup the currently set RenderTexture
       // RenderTexture previous = RenderTexture.active;

        // Set the current RenderTexture to the temporary one we created
       // RenderTexture.active = _RTexTemp;

        // Copy the pixels from the RenderTexture to the new Texture
        _Tex2D.ReadPixels(new Rect(0, 0, _RTex.width, _RTex.height), 0, 0);
        _Tex2D.Apply();



        // Reset the active RenderTexture
        // RenderTexture.active = previous;

        Profiler.BeginSample("Writing to leds");
        if (_LEDUVs.Count == 0)
        {
            foreach (var pixel in BlinkyBlinky.pixels)
                _LEDUVs.Add( _UVQuad.TransformPoint(pixel.UV) );
        }

        foreach (var pixel in BlinkyBlinky.pixels)
        {
            pixel.Color = _Tex2D.GetPixelBilinear(pixel.UV.x, pixel.UV.y) * 255;
        }
        Profiler.EndSample();
        
    }

    /*
    private void OnDrawGizmos()
    {       
        foreach (Vector3 vec in _LEDUVs)
        {
            Gizmos.DrawWireCube(vec, Vector3.one * 10);
        }
    }
    */
}
