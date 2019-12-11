using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.BlinkyLights;
using Unity.BlinkyBlinky;
using Unity.BlinkyShared.DMX;   
using Uki.Example.Animations;
using Unity.BlinkyBlinky.Animations;
using Klak.Ndi;
using UnityEngine.Profiling;

public class UKILightingManager : MonoBehaviour
{
    public enum AnimationSource
    {
        NDI,
        Animation,
    }

    string _SourceName; // either NDI or animation source name

    public static UKILightingManager Instance;

    public RenderTextureMapper _RenderTextureMapper;

    public Transform _InputTransform;

    // Describe the physical controllers we have.
   
    public DMXDeviceDetail PIXLITE_CONTROLLER = new DMXDeviceDetail("Pixlite16", "192.168.20.50", DMXProtocol.sACN);
    public DMXDeviceDetail ARTNET_CONTROLLERS = new DMXDeviceDetail("ArmourControllers", "127.0.0.1", DMXProtocol.Artnet);  //IP is a hack to get it to start without a contorller online. Arnet is always broadcast

    // Setup Fixtures on the PixliteController
    [HideInInspector]
    public short LEFT_WING_FIRST_UNIVERSE = 1;
    [HideInInspector]
    public short LEFT_WING_SECOND_UNIVERSE = 7;
    private static short RIGHT_WING_FIRST_UNIVERSE = 16;    // BIGS!!!!! WHAT THE HELLL!!!!! 
    private static short RIGHT_WING_SECOND_UNIVERSE = 11;   // Upper and lower cables swapped on right wing. dont swap back or mapping in Touch (fallback option) will be flipped in the wing. 

    private static short EYES_STARTING_UNIVERSE = 29;
    private static short LEGS_STARTING_UNIVERSE = 28;
    private static short FLOODS_STARTING_UNIVERSE = 27; 

    private static short ARMOUR_A_STARTING_UNIVERSE = 1;    // Artnet protocol is broadcast
    private static short ARMOUR_B_STARTING_UNIVERSE = 2;
    private static short ARMOUR_C_STARTING_UNIVERSE = 3;

    // Loop condition
    public static bool AnimationsRunning = true;

    public float _FrameRate = 30;

    AnimationSource _AnimSource;

    Fixture[] _FixtureArray;
    public Vector3 _DebugGizmoScale = new Vector3(.02f, .02f, .02f);
        
    NdiReceiver _NDIReciever;

    public ParticleSystem _LedPS;

    IBlinkyAnimation _Animation;

    #region INITIALIZATION

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _NDIReciever = GetComponent<NdiReceiver>();

        // Initialize Network And Controllers
        BlinkyBlinky.AddNetworkDevice(PIXLITE_CONTROLLER);
        BlinkyBlinky.AddNetworkDevice(ARTNET_CONTROLLERS);

        _FixtureArray = FindObjectsOfType<Fixture>();

        foreach (Fixture fixture in _FixtureArray)
            BlinkyBlinky.AddFixture(fixture.Init(this));

        Bounds b = new Bounds();
        foreach (var pixel in BlinkyBlinky.pixels)        
            b.Encapsulate(pixel.origin);

        foreach (var pixel in BlinkyBlinky.pixels)
            pixel.SetUV(b.min.x, b.max.x, b.min.y, b.max.y);

        _LedPS.maxParticles = BlinkyBlinky.pixels.Count;
        _LedPS.Emit(_LedPS.maxParticles);
        _LEDParticles = new ParticleSystem.Particle[_LedPS.main.maxParticles];

        _Animation = new XWash();
    }

    public void SetAnimSource(AnimationSource source, string name)
    {
        _AnimSource = source;

        if(_AnimSource == AnimationSource.NDI)
            _NDIReciever.sourceName = name;

        Debug.Log("LIGHTING MANAGER - Setting source too: " + source.ToString() + " - " + name);
    }
    #endregion

    #region ANIMATIONS
    public bool _UpdateAnimation = false;
    ParticleSystem.Particle[] _LEDParticles;
    public bool forceGC = false;
    private void FixedUpdate()
    {
        if (AnimationsRunning)
        {
            if (_AnimSource == AnimationSource.NDI)
            {
                Profiler.BeginSample("Updating redner texture mapper");
                _RenderTextureMapper.ManualUpdate();
               
                Profiler.EndSample();
            }
            else if (_AnimSource == AnimationSource.Animation)
            {
                Profiler.BeginSample("Updating animation");
                _Animation.Run();
               

                Profiler.EndSample();
            }

            Profiler.BeginSample("Updating BlinkyBlinky lights"); 
            BlinkyBlinky.UpdateLights();  // TODO has 282k GC
            Profiler.EndSample();

            // Profiler.BeginSample("Updating particles");
            UpdateParticles();
            // Profiler.EndSample();       
            
            if(forceGC)
                System.GC.Collect();
        }
    }

    void UpdateParticles()
    {
        Profiler.BeginSample("Update particles");
        // GetParticles is allocation free because we reuse the m_Particles buffer between updates
        int numParticlesAlive = _LedPS.GetParticles(_LEDParticles);

        // Change only the particles that are alive
        for (int i = 0; i < numParticlesAlive; i++)
        {
            _LEDParticles[i].position = BlinkyBlinky.pixels[i].currentLocation;
            _LEDParticles[i].startColor = BlinkyBlinky.pixels[i].Color;
        }

        // Apply the particle changes to the Particle System
        _LedPS.SetParticles(_LEDParticles, numParticlesAlive);
        Profiler.EndSample();
    }
        
    private void RunPlasmaAnimation()
    {
        var plasma = new PlasmaAnimation();

        var size = 1000000;
        var speed = 10000;
        var brightness = 20;

        plasma.Initialize(brightness, size, speed, true, true, true, true,true, false);

        _Animation = plasma;
    }
    #endregion
        
    #region FIXTURES
    /*
    private Fixture LeftWing()
    {
        var leftWing = new Fixture("LeftWing", PIXLITE_CONTROLLER);

        string filePath = Application.streamingAssetsPath + "/FixtureMappings/LeftWingUpper.csv";       
        leftWing.TryLoadLedChainFromFile(filePath, LEFT_WING_FIRST_UNIVERSE );
        leftWing.TryLoadLedChainFromFile(@".\Indexes\LeftWingLower.csv", LEFT_WING_SECOND_UNIVERSE );
            
        leftWing.SetFixtureOrigin(new Vector3(2000, 2500, 4000));
        //leftWing.RotateFixture(new Vector3(45, 0, 0));   //cant rotate outside unity

        print("Loading left wing fixture.");

        return leftWing;
    }

    private Fixture RightWing()
    {
        var rightWing = new Fixture("RightWing", PIXLITE_CONTROLLER);
        rightWing.TryLoadLedChainFromFile(@".\Indexes\RightWingUpper.csv", RIGHT_WING_FIRST_UNIVERSE  );
        rightWing.TryLoadLedChainFromFile(@".\Indexes\RightWingLower.csv", RIGHT_WING_SECOND_UNIVERSE);
        // rightWing.SetFixtureOrigin(new Vector3(2000, 2500, 4000));
        // rightWing.RotateFixture(new Vector3(45, 0, 0));
        return rightWing;
    }

    private Fixture Eyes()
    {
        var eyes = new Fixture("Eyes", PIXLITE_CONTROLLER);
        eyes.TryLoadLedChainFromFile(@".\Indexes\Eyes.csv", EYES_STARTING_UNIVERSE);//last string was on the left wing
        //eyes.SetFixtureOrigin(new Vector3(2000, 2500, 4000));
        // eyes.RotateFixture(new Vector3(0, 0, 0));

        return eyes;
    }

    private Fixture ArmourA()
    {
        //repeat Armour on 3 universes for 3 suits.
        var armourA = new Fixture("Armour", ARTNET_CONTROLLERS);
        armourA.TryLoadLedChainFromFile(@".\Indexes\Armour.csv", ARMOUR_A_STARTING_UNIVERSE);//last string was on the left wing
        return armourA;
    }

    private Fixture ArmourB()
    {
        var armourB = new Fixture("Armour", ARTNET_CONTROLLERS);
        armourB.TryLoadLedChainFromFile(@".\Indexes\Armour.csv", ARMOUR_B_STARTING_UNIVERSE);//last string was on the left wing
        return armourB;
    }

    private Fixture ArmourC()
    {
        var armourC = new Fixture("Armour", ARTNET_CONTROLLERS);
        armourC.TryLoadLedChainFromFile(@".\Indexes\Armour.csv", ARMOUR_C_STARTING_UNIVERSE);//last string was on the left wing
        return armourC;
    }

    private Fixture Flood()
    {
        //to make the floods less blinky on video feeds (from sampling one led), import the legs index, then average all the pixels.
        var flood = new Fixture("Floods", PIXLITE_CONTROLLER);
        flood.TryLoadLedChainFromFile(@".\Indexes\RightWingLower.csv", FLOODS_STARTING_UNIVERSE);//last string was on the left wing
            return flood;
    }

    private Fixture Legs()
    {
        var legs = new Fixture("Legs", PIXLITE_CONTROLLER);
        legs.TryLoadLedChainFromFile(@".\Indexes\Legs.csv", LEGS_STARTING_UNIVERSE);//last string was on the left wing
        return legs;
    }
    */

    private enum Is { Awesome}

    #endregion

    private void ConsolFirstPixel()
    {
        Console.WriteLine(BlinkyBlinky.pixels[0].R + " " + BlinkyBlinky.pixels[0].G + " " + BlinkyBlinky.pixels[0].B);
    }
}

