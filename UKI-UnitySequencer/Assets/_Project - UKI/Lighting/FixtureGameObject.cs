using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.BlinkyLights;
using Unity.BlinkyShared.DMX;

[System.Serializable]
public struct FixtureData
{
    public string _MappingFilename;
    public short _StartUniverse;
}

/// <summary>
/// A game objects that holds the fixture object and gives it a parent transform to update the LED positions
/// </summary>
public class FixtureGameObject : MonoBehaviour
{
    public enum ControllerType
    {
        PixliteController,
        ArtnetController,
    }

    public ControllerType _ControllerType = ControllerType.PixliteController;
    public FixtureData[] _FixtureData;

    Fixture _Fixture;

    bool isDirty = false;

    float _Scale = 1;
    float Scale
    {
        get { return _Scale; }
        set
        {
            if (_Scale != value)
                isDirty = true;

            _Scale = value;
        }
    }

    Vector3 _Position;
    Vector3 Position
    {
        get { return _Position; }
        set
        {
            if (_Position != value)
                isDirty = true;

            _Position = value;
        }
    }

    Vector3 _Rotation;
    Vector3 Rotation
    {
        get { return _Rotation; }
        set
        {
            if (_Rotation != value)
                isDirty = true;

            _Rotation = value;
        }
    }

    public bool _DrawDebug = false;
    public Vector3 _DebugScale = new Vector3(.02f, .02f, .02f);

    // Start is called before the first frame update
    public Fixture Init(UKILightingManager lightingTest)
    {
        DMXDeviceDetail controller = lightingTest.PIXLITE_CONTROLLER;
        switch (_ControllerType)
        {
            case ControllerType.PixliteController:
                controller = lightingTest.PIXLITE_CONTROLLER;
                break;
            case ControllerType.ArtnetController:
                controller = lightingTest.ARTNET_CONTROLLERS;
                break;
        }

        // Create fixture
        _Fixture = new Fixture(name, controller);

        for (int i = 0; i < _FixtureData.Length; i++)
        {
            string filePath = Application.streamingAssetsPath + "/FixtureMappings/" + _FixtureData[i]._MappingFilename;
            _Fixture.TryLoadLedChainFromFile(filePath, _FixtureData[i]._StartUniverse);
        }

        print("Fixture loaded: " + name);
        return _Fixture;
    }

    // Update is called once per frame
    public void Update()
    {
        Position = transform.position;
        Rotation = transform.localEulerAngles;
        Scale = transform.localScale.x;
       
        if(isDirty)
        {
            _Fixture.UpdateTransform(transform);
            isDirty = false;
        }
    }

    public void OnDrawGizmos()
    {
        if (_DrawDebug && Application.isPlaying)
        {
            foreach (LedChain ledChain in _Fixture.LedChains)
            {
                foreach (Pixel p in ledChain.Pixels)
                {
                    //Gizmos.color = Color.blue;
                    Gizmos.color =  p.color;
                    Gizmos.DrawWireCube(_Fixture.Origin + (p.currentLocation * .001f), _DebugScale);
                }
            }
        }
    }
}
