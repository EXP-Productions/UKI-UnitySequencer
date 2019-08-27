using ArtNet.Sockets;
using ArtNet.Packets;
using Unity.BlinkyNetworking.DMX;
using System;
using Unity.BlinkyShared.DMX;
using System.Collections.Generic;

namespace Unity.BlinkyNetworking.Artnet
{
    class ArtnetNetwork : DMXNetwork
    {
        private ArtNetSocket artnetSocket;
        private const int artnetPort = 6454;

        public ArtnetNetwork(DMXDeviceDetail device) : base(device)
        {
            //initialize Artnet Device
            artnetSocket = new ArtNetSocket();
            artnetSocket.EnableBroadcast = true;
            try
            {
                artnetSocket.Connect(device.IPAddress, artnetPort);
            }
            catch (Exception e)
            {
                Console.WriteLine("Trouble Connecting to the artnet controller. IP:" + device.IPAddress + " | " + e.ToString());
        
            }
        }

        public override void Send(DMXDatagram datagram)
        {
            ArtNetDmxPacket packet = new ArtNetDmxPacket();
            packet.DmxData = datagram.Buffer;
            packet.Universe = datagram.UniverseNo;
            artnetSocket.Send(packet);
        }

        public override string ToString()
        {
            return artnetSocket.ToString();
        }

        public override void Send(List<DMXDatagram> datagrams)
        {
            datagrams.ForEach(datagram => Send(datagram));
        }
    }
}
