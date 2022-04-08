using System;
using System.Collections.Concurrent;
using System.ComponentModel;

using System.Net;      //required
using System.Net.Sockets;    //required

namespace UkiConsole
{
    class TCPServer : Listener,Sender, INotifyPropertyChanged
    {


        private System.Net.Sockets.TcpListener _server;
        private TcpClient _tcpclient;
        private ConcurrentQueue<RawMove> _moveOut = new ConcurrentQueue<RawMove>();
        private ConcurrentQueue<RawMove> _movein = new ConcurrentQueue<RawMove>();

        private ConcurrentQueue<RawMove> _commandOut = new ConcurrentQueue<RawMove>();
        private bool _run = false;
        private bool _connected = false;
        private String _addr;
        private int _port;
        public event PropertyChangedEventHandler PropertyChanged;
        public NetworkStream _ns;
        public ConcurrentQueue<RawMove> MoveOut { get => _moveOut; }
        public ConcurrentQueue<RawMove> MoveIn { get => _movein; }

        public ConcurrentQueue<RawMove> CommandOut { get => _commandOut; }
        public bool listenerConnected { get => _connected; }
        public bool senderConnected { get => _connected; }

        protected void OnPropertyChanged(string propertyname)
        {
            PropertyChangedEventHandler eh = PropertyChanged;
            if (eh != null)
            {
                var en = new PropertyChangedEventArgs(propertyname);
                eh(this, en);
            }
        }
        private void checkConnection(bool state)
        {
            if (_connected != state)
            {
                // System.Diagnostics.Debug.WriteLine(String.Format("Changed connection: {0}", state));
                _connected = state;
                OnPropertyChanged("listenerConnected");

            }
        }
        public TCPServer(String addr, int port, ConcurrentQueue<RawMove> Moves, ConcurrentQueue<RawMove> Control)
        {
            _addr = addr;
            _port = port;
            // Config this, and add "local" vs "remote"
            if (_server is null)
            {
                _server = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Parse(_addr), _port);
                _server.Start();

            }
            else
            {
                _server.Stop();
                _server.Start();
            }




            _moveOut = Moves;
            _commandOut = Control;

        }

        public void Enqueue(RawMove mv)
        {

            MoveIn.Enqueue(mv);
        }


        public void Run()
        {
            _run = true;

            System.Diagnostics.Debug.WriteLine("Listening...");

            while (_run == true)   //we wait for a connection
            {
                try
                {
                    _tcpclient = _server.AcceptTcpClient();  //if a connection exists, the server will accept it

                    _ns = _tcpclient.GetStream();



                    while (_tcpclient.Connected is true)  //while the client is connected, we look for incoming messages
                    {

                        checkConnection(true);
                        while (_ns.DataAvailable)
                        {


                            try
                            {
                                //networkstream is used to send/receive messages

                                byte[] msg = new byte[6];     //the messages arrive as byte array
                                _ns.Read(msg, 0, msg.Length);   //the same networkstream reads the message sent by the client

                                UInt16 _addr = BitConverter.ToUInt16(msg, 0);

                                UInt16 reg = BitConverter.ToUInt16(msg, 2);
                                UInt16 val = BitConverter.ToUInt16(msg, 4);
                                System.Diagnostics.Debug.WriteLine("TCP MOVE: {0} : {1}, {2} ({3})", _addr.ToString(), reg, val, msg.Length);


                                RawMove _mv = new RawMove(_addr.ToString(), reg, val);



                                if (ModMap.ControlRegisters.Contains(reg) || ModMap.ControlAddresses.Contains(_addr))
                                {
                                    CommandOut.Enqueue(_mv);

                                }
                                else
                                {
                                    // System.Diagnostics.Debug.WriteLine("RECV MOVE: {0} : {1}, {2}", _addr.ToString(), reg, val);


                                    MoveOut.Enqueue(_mv);

                                }

                            }

                            catch (Exception ex)
                            {
                                _run = false;
                                System.Diagnostics.Debug.WriteLine("No TCP client connected");
                            }
                        }
                       
                        while (!MoveIn.IsEmpty)
                        {
                            RawMove _mv;
                           // System.Diagnostics.Debug.WriteLine("Sending TCP");
                            MoveIn.TryDequeue(out _mv);
                            if (_mv is not null)
                            {
                                sendStatus(_mv);
                            }
                        }
                        _ns.Flush();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Also messy...");
                }
                checkConnection(false);
                // _tcpclient.Close();
            }
            _server.Stop();

        }
        private void sendStatus(RawMove _mv)
        {

            if (_mv is not null)
            {
                System.Diagnostics.Debug.WriteLine("Sending TCP");
                // This should be a static singleton....
                byte[] data = new byte[6];
                byte[] _add = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(short.Parse(_mv.Addr)));
                byte[] _reg = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(_mv.Reg));
                byte[] _val = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(_mv.Val));
                 System.Diagnostics.Debug.WriteLine("TCP AXIS: " + _mv.Addr);
                //  System.Diagnostics.Debug.WriteLine("UDP REG: " + _mv.Reg.ToString());
                //  System.Diagnostics.Debug.WriteLine("UDP VAL: " + _mv.Val.ToString());
                data[0] = _add[1];
                data[1] = _add[0];


                // XXX Somewhere we are flipping network to host once too often.
                // This fixes that.
                data[2] = _reg[1];
                data[3] = _reg[0];
                data[4] = _val[1];
                data[5] = _val[0];


                Send(data);
            }

        }
        public void Send(byte[] message)
        {

            if (_ns is null)
            {
                System.Diagnostics.Debug.WriteLine("no stream");

                _ns = _tcpclient.GetStream();
            }
            System.Diagnostics.Debug.WriteLine("TCP trying");
            try
            {
                System.Diagnostics.Debug.WriteLine("TCP sending");

                _ns.Write(message, 0, message.Length);
                _ns.Flush();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("No TCP connection to send to");

            }

        }

    
    public void ShutDown()
        {
            _run = false;
            _server.Stop();

        }
    }
}