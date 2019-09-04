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

public class FixtureGameObject : MonoBehaviour
{
    public enum ControllerType
    {
        PixliteController,
        ArtnetController,
    }

    public ControllerType _ControllerType = ControllerType.PixliteController;

    Fixture _Fixture;

    public FixtureData[] _FixtureData;

    // Start is called before the first frame update
    public void Init(UKILightingManager lightingTest)
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
       
        _Fixture.ScaleFixture(transform.localScale.x);
        _Fixture.SetFixtureOrigin(transform.position);
        _Fixture.RotateFixture(transform.localEulerAngles);

        print("Fixture loaded: " + name);
    }

    // Update is called once per frame
    public void UpdateFixture()
    {
        
    }
}
