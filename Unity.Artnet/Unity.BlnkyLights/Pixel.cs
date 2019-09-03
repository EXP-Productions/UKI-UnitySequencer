using UnityEngine;

namespace Unity.BlinkyLights
{
    public class Pixel 
    {
        public Color color; 
        public Vector3 origin { get; private set; }
        public Vector3 location;

        public float R { get { return color.r; } set { color.r = value; } }
        public float G { get { return color.g; } set { color.g = value; } }
        public float B { get { return color.b; } set { color.b = value; } }

        public float X { get { return location.x; } set { location.x = value; } }
        public float Y { get { return location.y; } set { location.y = value; } }
        public float Z { get { return location.z; } set { location.x = value; } }

        public Pixel() { }

        public Pixel(Color c)
        {
            color = c;
        }

        public Pixel(Color c, Vector3 nativeLocation)
        {
            color = c ;
            origin = location = nativeLocation;
        }

        public Pixel(float x, float y)
        {
            origin = location = new Vector3(x, y);
        }

        public Pixel(Vector3 v)
        {
            origin = location = v;
        }

        public void Relocate(Vector3 relocationPoint)
        {
            location = relocationPoint;
        }

        public void Black()
        {
            color.r = color.g = color.b = 0;
        }
    }
}
