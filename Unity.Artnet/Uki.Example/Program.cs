using Unity.BlinkyLights;
using UnityEngine;
using Unity.BlinkyBlinky;
using Unity.BlinkyShared.DMX;   
using System;
using Uki.Example.Animations;
using Unity.BlinkyBlinky.Animations;
using System.Threading;

namespace Uki.Example
{
    public class Program
    {
        //describe the physical controllers we have.
        private static DMXDeviceDetail PIXLITE_CONTROLLER = new DMXDeviceDetail("Pixlite16", "192.168.20.50", DMXProtocol.sACN);
        private static DMXDeviceDetail ARTNET_CONTROLLERS = new DMXDeviceDetail("ArmourControllers", "127.0.0.1", DMXProtocol.Artnet);  //IP is a hack to get it to start without a contorller online. Arnet is always broadcast

        //Setup Fixtures on the PixliteController
        private static short LEFT_WING_FIRST_UNIVERSE = 1;
        private static short LEFT_WING_SECOND_UNIVERSE = 7;
        private static short RIGHT_WING_FIRST_UNIVERSE = 16; //BIGS!!!!! WHAT THE HELLL!!!!! 
        private static short RIGHT_WING_SECOND_UNIVERSE = 11; // Upper and lower cables swapped on right wing. dont swap back or mapping in Touch (fallback option) will be flipped in the wing. 

        private static short EYES_STARTING_UNIVERSE = 29;
        private static short LEGS_STARTING_UNIVERSE = 28;
        private static short FLOODS_STARTING_UNIVERSE = 27; 

        private static short ARMOUR_A_STARTING_UNIVERSE = 1; //artnet protocol is broadcast
        private static short ARMOUR_B_STARTING_UNIVERSE = 2;
        private static short ARMOUR_C_STARTING_UNIVERSE = 3;

        //FramerateLimit
        private static long framerate = 60;
        private static long ticksLast = DateTime.Now.Ticks;
        private static long ticksPerFrame = 1000 / 60 * 100000; //ms / rate * ticksPerMS


        //loop condition
        public static bool CallanIsAwesome = true;

        static void Main(string[] args)
        {
            InitializeNetworkAndControllers();
            LoadFixturesFromCSVs();

            //TestAnimation();
            RunPlasma();
            //One();
        }

        private static void One()
        {
            var animation = new OnePixel();
            while (CallanIsAwesome) //infinate loop
            {
                animation.Run();
                BlinkyBlinky.UpdateLights();
            }
        }

        private static void RunPlasma()
        {
            Console.WriteLine("Running Plasma Animation");

            var plasma = new PlasmaAnimation();

            var size = 100;
            var speed = 100;
            var brightness = 128;

            plasma.Initialize(brightness, size, speed, true, true, true, true,true, false);

            while (CallanIsAwesome) //infinate loop
            {
                plasma.Run();
                BlinkyBlinky.UpdateLights();

                LimitFramerate();
            }
        }

        private static void LimitFramerate()
        {

            var ticksNow = DateTime.Now.Ticks;
            var ticksDiff = ticksNow - ticksLast;//remaining ticks for framerate
            var sleep = Math.Max((int)(ticksPerFrame - ticksDiff) / 10000, 0) ;

            //Thread.Sleep(sleep);

            //Console.Clear();
            Console.WriteLine(" Sleep:" + sleep);
            ticksLast = ticksNow;
        }

        private static void TestAnimation()
        { 
            //repalce with managed system. Lots of work...better to accept a feed?
            var animation = new OneColorFixtureTest();
            while (CallanIsAwesome) //infinate loop
            {
                animation.Run();
                BlinkyBlinky.UpdateLights();
            }
        }

        private static void ConsolFirstPixel()
        {
            Console.WriteLine(BlinkyBlinky.pixels[0].R + " " + BlinkyBlinky.pixels[0].G + " " + BlinkyBlinky.pixels[0].B);
        }

        private static void InitializeNetworkAndControllers()
        {
            BlinkyBlinky.AddNetworkDevice(PIXLITE_CONTROLLER);
            BlinkyBlinky.AddNetworkDevice(ARTNET_CONTROLLERS);
        }

        private static void LoadFixturesFromCSVs()
        {
            //pixliteFixtures
            BlinkyBlinky.AddFixture(LeftWing());
            BlinkyBlinky.AddFixture(RightWing());
            BlinkyBlinky.AddFixture(Eyes());
            BlinkyBlinky.AddFixture(Flood());
            BlinkyBlinky.AddFixture(Legs());

            //artnet fixtures
              BlinkyBlinky.AddFixture(ArmourA());
              BlinkyBlinky.AddFixture(ArmourB());
              BlinkyBlinky.AddFixture(ArmourC());

        }

        #region Fixtures

        private static Fixture LeftWing()
        {
            var leftWing = new Fixture("LeftWing", PIXLITE_CONTROLLER);

            leftWing.TryLoadLedChainFromFile(@".\Indexes\LeftWingUpper.csv", LEFT_WING_FIRST_UNIVERSE );
            leftWing.TryLoadLedChainFromFile(@".\Indexes\LeftWingLower.csv", LEFT_WING_SECOND_UNIVERSE );
            
            leftWing.ScaleFixture(1.3f);
            leftWing.SetFixtureOrigin(new Vector3(2000, 2500, 4000));
            //leftWing.RotateFixture(new Vector3(45, 0, 0));   //cant rotate outside unity

            return leftWing;
        }

        private static Fixture RightWing()
        {
            var rightWing = new Fixture("RightWing", PIXLITE_CONTROLLER);
            rightWing.TryLoadLedChainFromFile(@".\Indexes\RightWingUpper.csv", RIGHT_WING_FIRST_UNIVERSE  );
            rightWing.TryLoadLedChainFromFile(@".\Indexes\RightWingLower.csv", RIGHT_WING_SECOND_UNIVERSE);
           // rightWing.SetFixtureOrigin(new Vector3(2000, 2500, 4000));
            // rightWing.RotateFixture(new Vector3(45, 0, 0));
            return rightWing;
        }

        private static Fixture Eyes()
        {
            var eyes = new Fixture("Eyes", PIXLITE_CONTROLLER);
            eyes.TryLoadLedChainFromFile(@".\Indexes\Eyes.csv", EYES_STARTING_UNIVERSE);//last string was on the left wing
            //eyes.SetFixtureOrigin(new Vector3(2000, 2500, 4000));
            // eyes.RotateFixture(new Vector3(0, 0, 0));

            return eyes;
        }

        private static Fixture ArmourA()
        {
            //repeat Armour on 3 universes for 3 suits.
            var armourA = new Fixture("Armour", ARTNET_CONTROLLERS);
            armourA.TryLoadLedChainFromFile(@".\Indexes\Armour.csv", ARMOUR_A_STARTING_UNIVERSE);//last string was on the left wing
            return armourA;
        }

        private static Fixture ArmourB()
        {
            var armourB = new Fixture("Armour", ARTNET_CONTROLLERS);
            armourB.TryLoadLedChainFromFile(@".\Indexes\Armour.csv", ARMOUR_B_STARTING_UNIVERSE);//last string was on the left wing
            return armourB;
        }

        private static Fixture ArmourC()
        {
            var armourC = new Fixture("Armour", ARTNET_CONTROLLERS);
            armourC.TryLoadLedChainFromFile(@".\Indexes\Armour.csv", ARMOUR_C_STARTING_UNIVERSE);//last string was on the left wing
            return armourC;
        }

        private static Fixture Flood()
        {
            //to make the floods less blinky on video feeds (from sampling one led), import the legs index, then average all the pixels.
            var flood = new Fixture("Floods", PIXLITE_CONTROLLER);
            flood.TryLoadLedChainFromFile(@".\Indexes\RightWingLower.csv", FLOODS_STARTING_UNIVERSE);//last string was on the left wing
             return flood;
        }

        private static Fixture Legs()
        {
            var legs = new Fixture("Legs", PIXLITE_CONTROLLER);
            legs.TryLoadLedChainFromFile(@".\Indexes\Legs.csv", LEGS_STARTING_UNIVERSE);//last string was on the left wing
            return legs;
        }

        private enum Is { Awesome}

        #endregion
    }
}
