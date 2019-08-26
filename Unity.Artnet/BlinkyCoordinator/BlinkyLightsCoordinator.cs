using Unity.BlinkyLights;
using Unity.BlinkyShared.DMX;
using Unity.BlinkyNetworking;
using System.Linq;

namespace Unity.BlinkyLightsCoordinator
{
    public class BlinkyCoordinator
    {
        public BlinkyNetwork Network;
        public BlinkyModel Model;

        public BlinkyCoordinator()
        {
            Network = new BlinkyNetwork();
            Model = new BlinkyModel();
        }

        public void AddNetworkDevice(DMXDeviceDetail device)
        {
            Network.AddNetworkDevice(device);
        }

        public void UpdateLights()
        {
            Model.Fixtures.ForEach(fixture =>
            Network.Networks.First(network => network.NetworkName == fixture.NetworkName)
                            .Send(DatagramComposer.GetDMXDatagrams(fixture)
                            ));
        }

    }
}
