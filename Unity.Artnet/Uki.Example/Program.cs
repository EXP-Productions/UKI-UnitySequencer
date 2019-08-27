using Unity.BlinkyLights;
using UnityEngine;
using Unity.BlinkyBlinky;
using Unity.BlinkyShared.DMX;   
using System;
using Uki.Example.Animations;

namespace Uki.Example
{
    public class Program
    {
        //describe the physical controllers we have.
        private static DMXDeviceDetail PIXLITE_CONTROLLER = new DMXDeviceDetail("Pixlite16", "192.168.2.50", DMXProtocol.sACN);
        private static DMXDeviceDetail ARTNET_CONTROLLER = new DMXDeviceDetail("ArtNetController", "192.168.2.100", DMXProtocol.Artnet);

        //Setup Fixtures on the PixliteController
        private static short LEFT_WING_STARTING_UNIVERSE = 1;
        private static short RIGHT_WING_STARTING_UNIVERSE = 8;
        private static short EYES_STARTING_UNIVERSE = 13;
   
        private static short LEGS_STARTING_UNIVERSE = 12;
        private static short FLOODS_STARTING_UNIVERSE_A = 0; //artnet
        private static short FLOODS_STARTING_UNIVERSE_B = 11; //sacn

        private static short ARMOUR_A_STARTING_UNIVERSE = 1;
        private static short ARMOUR_B_STARTING_UNIVERSE = 2;
        private static short ARMOUR_C_STARTING_UNIVERSE = 3;

        //loop condition
        public static bool CallanIsAwesome = true;

        static void Main(string[] args)
        {
            InitializeNetworkAndControllers();
            LoadFixturesFromCSVs();

            TestAnimation();
        }

        private static void TestAnimation()
        { 
            //repalce with managed system. Lots of work...better to accept a feed?
            var animation = new OneColorFixtureTest();
            int count = 1;
            long avg = 0;
            
            while (CallanIsAwesome)
            {
                animation.Run();

                BlinkyBlinky.UpdateLights();
            }
        }

        private static void InitializeNetworkAndControllers()
        {

            BlinkyBlinky.AddNetworkDevice(PIXLITE_CONTROLLER);
           // BlinkyCoordinator.AddNetworkDevice(ARTNET_CONTROLLER);
        }

        private static void LoadFixturesFromCSVs()
        {
            //pixliteFixtures
            BlinkyBlinky.AddFixture(LeftWing());
            BlinkyBlinky.AddFixture(RightWing());
            BlinkyBlinky.AddFixture(Eyes());

            //artnet fixtures
            //  BlinkyBlinky.AddFixture(ArmourA());
            //  BlinkyBlinky.AddFixture(ArmourB());
            //  BlinkyBlinky.AddFixture(ArmourC());
            //  BlinkyBlinky.AddFixture(Flood());
            //  BlinkyBlinky.AddFixture(Legs());
        }

        #region Fixtures

        private static Fixture LeftWing()
        {
            var leftWing = new Fixture("LeftWing", PIXLITE_CONTROLLER);
            leftWing.TryLoadLedChainFromFile(@".\Indexes\LeftWingUpper.csv", LEFT_WING_STARTING_UNIVERSE);
            leftWing.TryLoadLedChainFromFile(@".\Indexes\LeftWingLower.csv", leftWing.GetNextUniverse());

            leftWing.ScaleFixture(1.3f);
            leftWing.SetFixtureOrigin(new Vector3(2000, 2500, 4000));
            //leftWing.RotateFixture(new Vector3(45, 0, 0));   //cant rotate outside unity

            return leftWing;
        }

        private static Fixture RightWing()
        {
            var rightWing = new Fixture("RightWing", PIXLITE_CONTROLLER);
            rightWing.TryLoadLedChainFromFile(@".\Indexes\RightWingUpper.csv", RIGHT_WING_STARTING_UNIVERSE);
            rightWing.TryLoadLedChainFromFile(@".\Indexes\RightWingLower.csv", rightWing.GetNextUniverse());
            rightWing.SetFixtureOrigin(new Vector3(2000, 2500, 4000));
            // rightWing.RotateFixture(new Vector3(45, 0, 0));
            return rightWing;
        }

        private static Fixture Eyes()
        {
            var eyes = new Fixture("Eyes", PIXLITE_CONTROLLER);
            eyes.TryLoadLedChainFromFile(@".\Indexes\Eyes.csv", EYES_STARTING_UNIVERSE);//last string was on the left wing
            eyes.SetFixtureOrigin(new Vector3(2000, 2500, 4000));
            // eyes.RotateFixture(new Vector3(0, 0, 0));

            return eyes;
        }

        private static Fixture ArmourA()
        {
            //repeat Armour on 3 universes for 3 suits.
            var armourA = new Fixture("Armour", ARTNET_CONTROLLER);
            armourA.TryLoadLedChainFromFile(@".\Indexes\Armour.csv", ARMOUR_A_STARTING_UNIVERSE);//last string was on the left wing
            return armourA;
        }

        private static Fixture ArmourB()
        {
            var armourB = new Fixture("Armour", ARTNET_CONTROLLER);
            armourB.TryLoadLedChainFromFile(@".\Indexes\Armour.csv", ARMOUR_B_STARTING_UNIVERSE);//last string was on the left wing
            return armourB;
        }

        private static Fixture ArmourC()
        {
            var armourC = new Fixture("Armour", ARTNET_CONTROLLER);
            armourC.TryLoadLedChainFromFile(@".\Indexes\Armour.csv", ARMOUR_C_STARTING_UNIVERSE);//last string was on the left wing
            return armourC;
        }

        private static Fixture Flood()
        {
            //to make the floods less blinky on video feeds (from sampling one led), import the legs index, then average all the pixels.
            var flood = new Fixture("Flood", ARTNET_CONTROLLER);
            flood.TryLoadLedChainFromFile(@".\Indexes\Legs.csv", FLOODS_STARTING_UNIVERSE_A);//last string was on the left wing
             return flood;
        }

        private static Fixture Legs()
        {
            var legs = new Fixture("Legs", ARTNET_CONTROLLER);
            legs.TryLoadLedChainFromFile(@".\Indexes\Legs.csv", LEGS_STARTING_UNIVERSE);//last string was on the left wing
            return legs;
        }

        private enum Is { Awesome}

        #endregion
    }
}
