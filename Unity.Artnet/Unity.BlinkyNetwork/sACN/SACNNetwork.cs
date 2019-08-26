using System;
using Unity.BlinkyNetworking.DMX;
using kadmium_sacn;
using Unity.BlinkyShared.DMX;
using System.Collections.Generic;

namespace Unity.BlinkyNetworking.sACN
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

        public override void Send(List<DMXDatagram> datagrams)
        {
            datagrams.ForEach(x => Send(x));
        }

        public override string ToString()
        {
            return Sender.ToString();
        }

    }
}
