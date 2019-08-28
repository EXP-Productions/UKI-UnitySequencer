using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureMapper : MonoBehaviour
{
    public RenderTexture _RTex;

    public Vector2 _SampleUV;

    public int _SampleCount = 3000;
    Color[] _Cols;
    
    // Start is called before the first frame update
    void Start()
    {
        _Cols = new Color[_SampleCount];
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetMouseButton(0))
            return;

        RaycastHit hit;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            return;

        Renderer rend = hit.transform.GetComponent<Renderer>();
        MeshCollider meshCollider = hit.collider as MeshCollider;

        _SampleUV = hit.textureCoord;




        // Create a temporary RenderTexture of the same size as the texture
        RenderTexture tmp = RenderTexture.GetTemporary(
                        _RTex.width,
                        _RTex.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

        // Blit the pixels on texture to the RenderTexture
        Graphics.Blit(_RTex, tmp);

        // Backup the currently set RenderTexture
        RenderTexture previous = RenderTexture.active;

        // Set the current RenderTexture to the temporary one we created
        RenderTexture.active = tmp;

        // Create a new readable Texture2D to copy the pixels to it
        Texture2D myTexture2D = new Texture2D(_RTex.width, _RTex.height);

        // Copy the pixels from the RenderTexture to the new Texture
        myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        myTexture2D.Apply();

        // Reset the active RenderTexture
        RenderTexture.active = previous;

        // Release the temporary RenderTexture
        RenderTexture.ReleaseTemporary(tmp);

        // "myTexture2D" now has the same pixels from "texture" and it's readable.

       
        for (int i = 0; i < _Cols.Length; i++)
        {
            _Cols[i] = myTexture2D.GetPixelBilinear(_SampleUV.x + (Random.value * .05f), _SampleUV.y + (Random.value * .05f));
        }
    }
}
