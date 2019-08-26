
using Unity.BlinkyLights;
using Unity.BlinkyNetwork;
using Unity.BlinkyNetwork.DMX;

namespace Unity.BlinkyLightsCoordinator
{
    public class BlinkyCoordinator
    {
        public BlinkyNetworkManager BlinkyNetwork;
        public BlinkyModel BlinkyModel;

        public BlinkyCoordinator()
        {
            BlinkyNetwork = new BlinkyNetworkManager();
            BlinkyModel = new BlinkyModel();
        }

        public void AddNetworkDevice(DMXDeviceDetail device)
        {
            BlinkyNetwork.AddNetworkDevice(device);
        }

        public void UpdateLights()
        {

        }

    }
}
