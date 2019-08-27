using Unity.BlinkyLights;
using Unity.BlinkyShared.DMX;
using Unity.BlinkyNetworking;
using System.Linq;
using System.Collections.Generic;

namespace Unity.BlinkyBlinky
{
    public static unsafe class BlinkyBlinky
    {
        private static BlinkyNetwork Network = new BlinkyNetwork();
        private static BlinkyModel Model = new BlinkyModel();

        //accessors
        public static List<Pixel> pixels = new List<Pixel>();
        public static List<BlinkyFixture> fixtures = new List<BlinkyFixture>();

        //workers
        public static void AllBlack() => Model.AllBlack();

        public static void AddNetworkDevice(DMXDeviceDetail device)
        {
            Network.AddNetworkDevice(device);
        }

        public static void AddFixture(Fixture fixture)
        {
            var result =  Model.AddFixture(fixture); //the result is a copy of the new fixture

            //copy the result to the two main arrays above for easy access.
            var newFixture = new BlinkyFixture(result);
            fixtures.Add(newFixture);
            pixels.AddRange(newFixture.pixels);
        }

        public static void UpdateLights()
        {
            Model.Fixtures.ForEach(fixture =>
            Network.Networks.First(network => network.NetworkName == fixture.NetworkName)
                            .Send(DatagramComposer.GetDMXDatagrams(fixture)
                            ));
        }
    }
}
