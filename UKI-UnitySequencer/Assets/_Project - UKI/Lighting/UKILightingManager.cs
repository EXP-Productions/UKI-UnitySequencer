using System;
using System.Collections;
using UnityEngine;
using Unity.BlinkyLights;
using Unity.BlinkyBlinky;
using Unity.BlinkyShared.DMX;   
using Uki.Example.Animations;
using Unity.BlinkyBlinky.Animations;

public class UKILightingManager : MonoBehaviour
{
    // Describe the physical controllers we have.
    public DMXDeviceDetail PIXLITE_CONTROLLER = new DMXDeviceDetail("Pixlite16", "192.168.20.50", DMXProtocol.sACN);
    public DMXDeviceDetail ARTNET_CONTROLLERS = new DMXDeviceDetail("ArmourControllers", "127.0.0.1", DMXProtocol.Artnet);  //IP is a hack to get it to start without a contorller online. Arnet is always broadcast

    // Setup Fixtures on the PixliteController
    public short LEFT_WING_FIRST_UNIVERSE = 1;
    public short LEFT_WING_SECOND_UNIVERSE = 7;
    private static short RIGHT_WING_FIRST_UNIVERSE = 16;    // BIGS!!!!! WHAT THE HELLL!!!!! 
    private static short RIGHT_WING_SECOND_UNIVERSE = 11;   // Upper and lower cables swapped on right wing. dont swap back or mapping in Touch (fallback option) will be flipped in the wing. 

    private static short EYES_STARTING_UNIVERSE = 29;
    private static short LEGS_STARTING_UNIVERSE = 28;
    private static short FLOODS_STARTING_UNIVERSE = 27; 

    private static short ARMOUR_A_STARTING_UNIVERSE = 1;    // Artnet protocol is broadcast
    private static short ARMOUR_B_STARTING_UNIVERSE = 2;
    private static short ARMOUR_C_STARTING_UNIVERSE = 3;

    // FramerateLimit
    private static long framerate = 60;
    private static long ticksLast = DateTime.Now.Ticks;
    private static long ticksPerFrame = 1000 / 60 * 100000; // ms / rate * ticksPerMS

    // Fixture parents
        
        
    // Loop condition
    public static bool CallanIsAwesome = true;

    public float _FrameRate = 30;

    public FixtureGameObject[] _FixtureArray;

    #region INITIALIZATION

    void Start()
    {
        InitializeNetworkAndControllers();

        _FixtureArray = FindObjectsOfType<FixtureGameObject>();

        foreach (FixtureGameObject fixture in _FixtureArray)
            BlinkyBlinky.AddFixture(fixture.Init(this));

        //LoadFixturesFromCSVs();

        //RunTestAnimation();
        RunPlasmaAnimation();
        //RunOneAnimation();
    }

    private void InitializeNetworkAndControllers()
    {
        BlinkyBlinky.AddNetworkDevice(PIXLITE_CONTROLLER);
        BlinkyBlinky.AddNetworkDevice(ARTNET_CONTROLLERS);
    }

    private void LoadFixturesFromCSVs()
    {
        //pixliteFixtures
        //BlinkyBlinky.AddFixture(LeftWing());
        //BlinkyBlinky.AddFixture(RightWing());
        //BlinkyBlinky.AddFixture(Eyes());
        //BlinkyBlinky.AddFixture(Flood());
        //BlinkyBlinky.AddFixture(Legs());

        //artnet fixtures
        //BlinkyBlinky.AddFixture(ArmourA());
        //BlinkyBlinky.AddFixture(ArmourB());
        //BlinkyBlinky.AddFixture(ArmourC());
    }

    #endregion

    #region ANIMATIONS

    IEnumerator AnimationRoutine(float fps, IBlinkyAnimation animation)
    {
        float wait = 1f / fps;

        while(CallanIsAwesome)
        {
            animation.Run();
            BlinkyBlinky.UpdateLights();

            yield return new WaitForSeconds(wait);
        }
    }
        
    private void RunTestAnimation()
    {
        StartCoroutine(AnimationRoutine(_FrameRate, new OneColorFixtureTest()));
    }

    private void RunPlasmaAnimation()
    {
        var plasma = new PlasmaAnimation();

        var size = 100;
        var speed = 10000;
        var brightness = 20;

        plasma.Initialize(brightness, size, speed, true, true, true, true,true, false);

        StartCoroutine(AnimationRoutine(_FrameRate, plasma));
    }

    private void RunOneAnimation()
    {
        StartCoroutine(AnimationRoutine(_FrameRate, new OnePixel()));
    }

    #endregion
        
    #region FIXTURES

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

    private enum Is { Awesome}

    #endregion

    private void ConsolFirstPixel()
    {
        Console.WriteLine(BlinkyBlinky.pixels[0].R + " " + BlinkyBlinky.pixels[0].G + " " + BlinkyBlinky.pixels[0].B);
    }
}

