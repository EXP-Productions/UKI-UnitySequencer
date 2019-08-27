using System;
using UnityEngine;

namespace Unity.BlinkyLights
{
    public class Pixel
    {
        public Color color;
        public Vector3 origin { get; private set; }
        public Vector3 location { get; private set; }

        public float r => color.r;
        public float g => color.g;
        public float b => color.b;

        public float x => location.x;
        public float y => location.y;
        public float z => location.z;

        public Pixel() { }

        public Pixel(Color c)
        {
            color = c;
        }

        public Pixel(Color c, Vector3 v)
        {
            color = c;
            origin = location = v;
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
