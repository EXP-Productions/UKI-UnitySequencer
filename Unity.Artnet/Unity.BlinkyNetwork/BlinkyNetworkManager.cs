using Unity.BlinkyNetwork.DMX;
using System.Collections.Generic;

namespace Unity.BlinkyNetwork
{
    public class NetworkManager
    {
        private DmxNetworkManager networkManager;

        public NetworkManager()
        {
            networkManager = new DmxNetworkManager();
        }

        public void AddNetworkDevice( string name, string ipAddress, DMXProtocol networkType)
        {
             networkManager.AddNetworkDevice(name, ipAddress, networkType);
        }

        public IEnumerable<DMXDeviceDetail> GetRegisteredNetworkDevices()
        {
            return networkManager.ListRegisteredDevices();
        }
    }

}
