using System;
using System.Collections.Generic;
using Unity.BlinkyNetwork.sACN;
using Unity.BlinkyNetwork.Artnet;
using System.Linq;

namespace Unity.BlinkyNetwork.DMX
{
    public class DmxNetworkManager
    {
        public IList<DMXNetwork> Networks;

        public DmxNetworkManager()
        {
            Networks = new List<DMXNetwork>();
        }

        public void AddNetworkDevice(DMXDeviceDetail device )
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
                result.Add(net.DeviceDetail) ;
            }
            return result;
        }

        public void UpdateAllLights()
        {

        }

        private void Send(DMXDatagram datagram)
        {
            try
            {
                if(Networks.Count > 0) Networks.First(x => x.DeviceDetail.NetworkName == datagram.NetworkName).Send(datagram);
            }
            catch { Console.WriteLine("Error sending DMXDatagram: Network Not Initialized: " + datagram.NetworkName); }
        }
    }
}
