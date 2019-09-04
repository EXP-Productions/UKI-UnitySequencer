using UnityEngine;

namespace Unity.BlinkyLights
{
    public class Pixel 
    {
        public Color color; 
        public Vector3 origin { get; private set; }
        public Vector3 currentLocation;

        public float R { get { return color.r; } set { color.r = value; } }
        public float G { get { return color.g; } set { color.g = value; } }
        public float B { get { return color.b; } set { color.b = value; } }

        public float X { get { return currentLocation.x; } set { currentLocation.x = value; } }
        public float Y { get { return currentLocation.y; } set { currentLocation.y = value; } }
        public float Z { get { return currentLocation.z; } set { currentLocation.x = value; } }

        public Pixel() { }

        public Pixel(Color c)
        {
            color = c;
        }

        public Pixel(Color c, Vector3 nativeLocation)
        {
            color = c ;
            origin = currentLocation = nativeLocation;
        }

        public Pixel(float x, float y)
        {
            origin = currentLocation = new Vector3(x, y);
        }

        public Pixel(Vector3 v)
        {
            origin = currentLocation = v;
        }

        public void UpdateLocation(Transform parentTransform)
        {
            currentLocation = parentTransform.TransformPoint(origin);
        }

        public void Black()
        {
            color.r = color.g = color.b = 0;
        }
    }
}
