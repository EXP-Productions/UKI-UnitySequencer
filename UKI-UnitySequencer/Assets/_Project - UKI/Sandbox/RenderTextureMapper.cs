using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.BlinkyBlinky;

public class RenderTextureMapper : MonoBehaviour
{
    public RenderTexture _RTex;
    public Vector2 _SampleUV;
    public int _SampleCount = 3000;
    Color[] _Cols;

    public Transform _UVQuad;
    List<Vector3> _LEDUVs = new List<Vector3>();

    RenderTexture _RTexTemp;

    // Start is called before the first frame update
    void Start()
    {
        _Cols = new Color[_SampleCount];

        _RTexTemp = RenderTexture.GetTemporary(
                        _RTex.width,
                        _RTex.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);
    }

    // Update is called once per frame
    public void ManualUpdate()
    {
        // Blit the pixels on texture to the RenderTexture
        Graphics.Blit(_RTex, _RTexTemp);

        // Backup the currently set RenderTexture
        RenderTexture previous = RenderTexture.active;

        // Set the current RenderTexture to the temporary one we created
        RenderTexture.active = _RTexTemp;

        // Create a new readable Texture2D to copy the pixels to it
        Texture2D myTexture2D = new Texture2D(_RTex.width, _RTex.height);

        // Copy the pixels from the RenderTexture to the new Texture
        myTexture2D.ReadPixels(new Rect(0, 0, _RTexTemp.width, _RTexTemp.height), 0, 0);
        myTexture2D.Apply();

        // Reset the active RenderTexture
        RenderTexture.active = previous;

        // Release the temporary RenderTexture
        //RenderTexture.ReleaseTemporary(tmp);

        // "myTexture2D" now has the same pixels from "texture" and it's readable.

        if(_LEDUVs.Count == 0)
        {
            foreach (var pixel in BlinkyBlinky.pixels)
                _LEDUVs.Add( _UVQuad.TransformPoint(pixel.UV) );
        }

        foreach (var pixel in BlinkyBlinky.pixels)
        {
            pixel.color = myTexture2D.GetPixelBilinear(pixel.UV.x, pixel.UV.y) * 255;
        }        
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
