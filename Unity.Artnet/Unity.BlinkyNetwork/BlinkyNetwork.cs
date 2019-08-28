using Unity.BlinkyNetworking.DMX;
using System.Collections.Generic;
using System;
using Unity.BlinkyShared.DMX;
using Unity.BlinkyNetworking.Artnet;
using Unity.BlinkyNetworking.sACN;
using System.Linq;

namespace Unity.BlinkyNetworking
{
    public class BlinkyNetwork
    {
        public IList<DMXNetwork> Networks;

        public BlinkyNetwork()
        {
            Networks = new List<DMXNetwork>();
        }

        public void AddNetworkDevice(DMXDeviceDetail device)
        {

            switch (device.Protocol)
            {
                case DMXProtocol.Artnet:
                    Networks.Add(new ArtnetNetwork(device));
                    break;
                case DMXProtocol.sACN:
                    Networks.Add(new SACNNetwork(device));
                    break;
            }
        }

        public IEnumerable<DMXDeviceDetail> ListRegisteredDevices()
        {
            var result = new List<DMXDeviceDetail>();
            foreach (var net in Networks)
            {
                result.Add(net.DeviceDetail);
            }
            return result;
        }

        private void Send(DMXDatagram datagram)
        {
            try
            {
                if (Networks.Count > 0) Networks.First(x => x.DeviceDetail.NetworkName == datagram.NetworkName).Send(datagram);
            }
            catch { Console.WriteLine("Error sending DMXDatagram: Network Not Initialized: " + datagram.NetworkName); }
        }

    }

}
