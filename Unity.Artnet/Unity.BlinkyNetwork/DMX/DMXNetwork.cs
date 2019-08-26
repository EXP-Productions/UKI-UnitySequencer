using Unity.BlinkyShared.DMX;

namespace Unity.BlinkyNetwork.DMX
{
    public interface IDMXNetwork { }

    public abstract class DMXNetwork : IDMXNetwork 
    {
        public DMXDeviceDetail DeviceDetail { get; private set; }

        public DMXNetwork(DMXDeviceDetail device)
        {
            DeviceDetail = device;
        }

        public abstract void Send(DMXDatagram datagram);
        
    }
}
