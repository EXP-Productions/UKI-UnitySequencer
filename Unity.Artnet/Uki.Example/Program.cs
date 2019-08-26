using Unity.BlinkyLights;
using UnityEngine;
using Unity.BlinkyLightsCoordinator;
using Unity.BlinkyShared.DMX;

namespace Uki.Example
{
    public class Program
    {
        //describe the physical controllers we have.
        private static DMXDeviceDetail pixliteController = new DMXDeviceDetail("Pixlite16", "192.168.2.50", DMXProtocol.sACN);
        private static DMXDeviceDetail artnetController = new DMXDeviceDetail("ArtNetController", "192.168.2.100", DMXProtocol.Artnet);

        public static BlinkyCoordinator blinkyCoordinator;

        //Setup Fixtures on the PixliteController
        private static int LEFT_WING_STARTING_UNIVERSE = 1;
        private static int RIGHT_WING_STARTING_UNIVERSE = 6;
        private static int EYES_STARTING_UNIVERSE = 13;
   
        private static int LEGS_STARTING_UNIVERSE = 12;
        private static int FLOODS_STARTING_UNIVERSE_A = 0; //artnet
        private static int FLOODS_STARTING_UNIVERSE_B = 11; //sacn

        private static int ARMOUR_A_STARTING_UNIVERSE = 1;
        private static int ARMOUR_B_STARTING_UNIVERSE = 2;
        private static int ARMOUR_C_STARTING_UNIVERSE = 3;

        static void Main(string[] args)
        {
            blinkyCoordinator = new BlinkyCoordinator();

            InitializeNetworkAndControllerss();        
            LoadFixturesOnPixliteController(blinkyCoordinator.BlinkyModel);
            LoadFixturesOnArtnetController(blinkyCoordinator.BlinkyModel);

            blinkyCoordinator.UpdateLights();

        }

        private static void InitializeNetworkAndControllerss()
        {
            blinkyCoordinator.AddNetworkDevice(pixliteController);
            blinkyCoordinator.AddNetworkDevice(artnetController);
        }

        private static void LoadFixturesOnPixliteController(BlinkyModel model)
        {

            var leftWing = new Fixture("LeftWing", pixliteController);                     
            leftWing.TryLoadLedChainFromFile(@".\Indexes\LeftWingUpper.csv", LEFT_WING_STARTING_UNIVERSE);  
            leftWing.TryLoadLedChainFromFile(@".\Indexes\LeftWingLower.csv", leftWing.GetNextUniverse());   
            
            //scale, locate and rotate fixture
            leftWing.ScaleFixture(1.3f);
            leftWing.SetFixtureOrigin(new Vector3(2000, 2500, 4000));
            //leftWing.RotateFixture(new Vector3(45, 0, 0));   //cant rotate outside unity

            //add to the model.
            model.AddFixture(leftWing);      


            var rightWing = new Fixture("RightWing", pixliteController);
            rightWing.TryLoadLedChainFromFile(@".\Indexes\RightWingUpper.csv", RIGHT_WING_STARTING_UNIVERSE);
            rightWing.TryLoadLedChainFromFile(@".\Indexes\RightWingLower.csv", rightWing.GetNextUniverse());
            rightWing.SetFixtureOrigin(new Vector3(2000, 2500, 4000));
           // rightWing.RotateFixture(new Vector3(45, 0, 0));
            model.AddFixture(rightWing);

            var eyes = new Fixture("Eyes", pixliteController);
            eyes.TryLoadLedChainFromFile(@".\Indexes\Eyes.csv", EYES_STARTING_UNIVERSE);//last string was on the left wing
            eyes.SetFixtureOrigin(new Vector3(2000, 2500, 4000));
           // eyes.RotateFixture(new Vector3(0, 0, 0));
            model.AddFixture(eyes);

        }

        private static void LoadFixturesOnArtnetController(BlinkyModel model)
        {
            //repeat Armour on 3 universes for 3 suits.
            var armourA = new Fixture("Armour", artnetController);
            armourA.TryLoadLedChainFromFile(@".\Indexes\Armour.csv", ARMOUR_A_STARTING_UNIVERSE);//last string was on the left wing
            model.AddFixture(armourA);

            var armourB = new Fixture("Armour", artnetController);
            armourB.TryLoadLedChainFromFile(@".\Indexes\Armour.csv", ARMOUR_B_STARTING_UNIVERSE);//last string was on the left wing
            model.AddFixture(armourB);

            var armourC = new Fixture("Armour", artnetController);
            armourC.TryLoadLedChainFromFile(@".\Indexes\Armour.csv", ARMOUR_C_STARTING_UNIVERSE);//last string was on the left wing
            model.AddFixture(armourC);
            
            //to make the floods less blinky on video feeds (from sampling one led), import the legs index, then average all the pixels.
            var flood = new Fixture("Flood", artnetController);
            flood.TryLoadLedChainFromFile(@".\Indexes\Legs.csv", FLOODS_STARTING_UNIVERSE_A);//last string was on the left wing
            model.AddFixture(flood);
            
            var legs = new Fixture("Legs", artnetController);
            legs.TryLoadLedChainFromFile(@".\Indexes\Legs.csv", LEGS_STARTING_UNIVERSE);//last string was on the left wing
            model.AddFixture(legs);
        }
    }
}
