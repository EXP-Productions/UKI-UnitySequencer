
using System.Linq;
using Unity.BlinkyNetwork;
using Unity.BlinkyNetwork.DMX;
using Unity.BlinkyLights;
using System;

namespace Uki.Example
{
    public class Program
    {
        private static DMXDeviceDetail pixliteController = new DMXDeviceDetail("Pixlite", "192.168.2.50", DMXProtocol.sACN);
        private static BlinkyNetworkManager blinkyNetwork;

        static void Main(string[] args)
        {
            var model = new Model();

            InitializeNetwork();        
            LoadFixturesOnPixliteController(model);

            // model.Fixtures.Add();
        }

        private static void LoadFixturesOnPixliteController(Model model)
        {
            //Setup Fixtures on the PixliteController
            var WING_STARTING_UNIVERSE = 1;

            var leftWing = new Fixture("LeftWing", pixliteController);                      //create a new fixture, and tell it what controller it is attached to.
            leftWing.TryLoadLedChainFromFile(@".\Indexes\LeftWingUpper.csv", WING_STARTING_UNIVERSE);             //load the indexfiles
            leftWing.TryLoadLedChainFromFile(@".\Indexes\LeftWingLower.csv", leftWing.GetNextUniverse());   //load the indexfiles
            model.Fixtures.Add(leftWing);      

            var rightWing = new Fixture("RightWing", pixliteController);
            rightWing.TryLoadLedChainFromFile(@".\Indexes\RightWingUpper.csv", leftWing.GetNextUniverse());//last string was on the left wing
            rightWing.TryLoadLedChainFromFile(@".\Indexes\RightWingLower.csv", rightWing.GetNextUniverse());//now the last string is on the right wing
            model.Fixtures.Add(rightWing);

            
        }

        private static void InitializeNetwork()
        {
            blinkyNetwork = new BlinkyNetworkManager();

            blinkyNetwork.AddNetworkDevice(pixliteController);
            blinkyNetwork.DMXNetworkManager.Networks.First(x => x.DeviceDetail.NetworkName == "Pixlite");
        }

    }
}
