using Unity.BlinkyNetwork.DMX;
using System.Collections.Generic;
using System;
using Unity.BlinkyShared.DMX;

namespace Unity.BlinkyNetwork
{
    public class BlinkyNetworkManager
    {
        public DmxNetworkManager DMXNetworkManager;

        public BlinkyNetworkManager()
        {
            DMXNetworkManager = new DmxNetworkManager();
        }

        public void AddNetworkDevice( DMXDeviceDetail detail)
        {
            DMXNetworkManager.AddNetworkDevice(detail);
        }

        public IEnumerable<DMXDeviceDetail> GetRegisteredNetworkDevices()
        {
            return DMXNetworkManager.ListRegisteredDevices();
        }

    }

}
