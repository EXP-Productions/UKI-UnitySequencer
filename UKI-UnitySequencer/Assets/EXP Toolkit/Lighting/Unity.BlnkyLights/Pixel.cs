using UnityEngine;

namespace Unity.BlinkyLights
{
    public class Pixel 
    {
        Color _CurrentColor;
        Color _TargetCol;
        public float _Smoothing = 0;
        public Color Color
        {
            get { return _CurrentColor; }
            set
            {
                if (_Smoothing == 0)
                    _CurrentColor = value;
                else
                {
                    _TargetCol = value;
                    _CurrentColor = Color.Lerp(_CurrentColor, _TargetCol, Time.deltaTime * _Smoothing);
                }
            }
        }
        public Vector3 origin { get; private set; }
        public Vector3 offset { get; private set; }
        public Vector2 UV { get; private set; }
        public Vector3 currentLocation;

        public float R { get { return _CurrentColor.r; } set { _CurrentColor.r = value; } }
        public float G { get { return _CurrentColor.g; } set { _CurrentColor.g = value; } }
        public float B { get { return _CurrentColor.b; } set { _CurrentColor.b = value; } }

        public float X { get { return currentLocation.x; } set { currentLocation.x = value; } }
        public float Y { get { return currentLocation.y; } set { currentLocation.y = value; } }
        public float Z { get { return currentLocation.z; } set { currentLocation.x = value; } }

        public float _DistFromInputT = 0;


        public Pixel(float x, float y, float smoothing = 0)
        {
            _Smoothing = smoothing;
            origin = currentLocation = new Vector3(x, y);
        }

        public void UpdateLocation(Transform parentTransform, Transform inputT)
        {
            currentLocation = parentTransform.TransformPoint(origin + offset);
            _DistFromInputT = Vector3.Distance(currentLocation, inputT.position);
        }

        public void SetUV(float minX = 0, float maxX = 1, float minY = 0, float maxY = 1 )
        {
            UV = new Vector2(Mathf.InverseLerp(minX, maxX, origin.x), Mathf.InverseLerp(minY, maxY, origin.y));
        }

        public void Black()
        {
            _CurrentColor.r = _CurrentColor.g = _CurrentColor.b = 0;
        }
    }
}
