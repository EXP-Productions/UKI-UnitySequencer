using System;
using Unity.BlinkyNetwork.DMX;
using kadmium_sacn;
using Unity.BlinkyShared.DMX;

namespace Unity.BlinkyNetwork.sACN
{
    public class SACNNetwork : DMXNetwork
    {
        private SACNSender Sender;

        public SACNNetwork(DMXDeviceDetail device) : base(device)
        {
            Sender = new SACNSender(new Guid(), device.NetworkName);
            Sender.UnicastAddress = device.IPAddress;
        }

        public override void Send(DMXDatagram data)
        {
            Sender.Send(data.UniverseNo, data.Buffer);
        }

        public override string ToString()
        {
            return Sender.ToString();
        }

    }
}
