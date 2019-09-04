using System.Net;

namespace Unity.BlinkyShared.DMX
{
    public class DMXDeviceDetail
    {
        public IPAddress IPAddress { get; private set; }
        public string NetworkName { get; private set; }
        public DMXProtocol Protocol { get; private set; }

        public DMXDeviceDetail(string networkName, string ipaddress, DMXProtocol protocol)
        {
            IPAddress ip;
            try
            {
                ip = IPAddress.Parse(ipaddress);
                NetworkName = networkName;
                IPAddress = ip;
                Protocol = protocol;
            }
            catch
            {
                throw new System.Exception("IP address malformed: " + ipaddress + " + , networkName: " + networkName);
            }
        }
    }
}