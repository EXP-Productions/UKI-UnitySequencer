using System.Collections.Generic;
using Unity.BlinkyShared.DMX;

namespace Unity.BlinkyNetworking.DMX
{
    public interface IDMXNetwork { }

    public abstract class DMXNetwork : IDMXNetwork 
    {
        public DMXDeviceDetail DeviceDetail { get; private set; }

        public DMXNetwork(DMXDeviceDetail device)
        {
            DeviceDetail = device;
        }

        public string NetworkName => DeviceDetail.NetworkName;

        public abstract void Send(DMXDatagram datagram);

        public abstract void Send(List<DMXDatagram> datagrams);

    }
}
