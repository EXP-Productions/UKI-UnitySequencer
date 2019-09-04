using UnityEngine;

namespace Unity.BlinkyLights
{
    public class Pixel 
    {
        public Color color; 
        public Vector3 origin { get; private set; }
        public Vector3 offset { get; private set; }
        public Vector2 UV { get; private set; }
        public Vector3 currentLocation;

        public float R { get { return color.r; } set { color.r = value; } }
        public float G { get { return color.g; } set { color.g = value; } }
        public float B { get { return color.b; } set { color.b = value; } }

        public float X { get { return currentLocation.x; } set { currentLocation.x = value; } }
        public float Y { get { return currentLocation.y; } set { currentLocation.y = value; } }
        public float Z { get { return currentLocation.z; } set { currentLocation.x = value; } }

        public Pixel(float x, float y)
        {
            origin = currentLocation = new Vector3(x, y);
        }

        public void UpdateLocation(Transform parentTransform)
        {
            currentLocation = parentTransform.TransformPoint(origin + offset);
        }

        public void SetUV(float minX = 0, float maxX = 1, float minY = 0, float maxY = 1 )
        {
            UV = new Vector2(Mathf.InverseLerp(minX, maxX, origin.x), Mathf.InverseLerp(minY, maxY, origin.y));
        }

        public void Black()
        {
            color.r = color.g = color.b = 0;
        }
    }
}
