﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using System.Collections.Concurrent;
using System.ComponentModel;



namespace UkiConsole
{
    class CommsManager //: INotifyPropertyChanged
    {
        private iListener _listener;
        private SendWrapper _sender;
        private Dictionary<String, ModbusManager> _myManagers = new();
        private string _mode ;
        private ConcurrentQueue<RawMove> _control = new ConcurrentQueue<RawMove>();
        private ConcurrentQueue<RawMove> _rawIn = new ConcurrentQueue<RawMove>();
        private AxisManager _axes;
        private bool _run = false;
        private ShowRunner _showrunner;
        private int _listenerPort = 9000;

        private int _senderPort = 10001;
        private String _senderAddr = "127.0.0.1";

        private String _listenerAddr = "127.0.0.1";
        private bool _senderConnected = false;
        public event PropertyChangedEventHandler PropertyChanged;
       

        //TODO - pub/sub model for connected status


        /* Use this somewhere ....
           IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            String _myaddr = "";
            foreach (IPAddress addr in localIPs)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (_myaddr.Equals(""))
                    {
                        // really, any 
                        _myaddr = addr.ToString();
                    }
                }
            }
        */
        public CommsManager(string Mode,  ShowRunner Runner, Dictionary<String, ModbusManager> Managers )
        {
            _mode = Mode;
            _showrunner = Runner;
            _axes = _showrunner.Axes;
            _myManagers = Managers;

            spawn_comms();
           
            startManagers();
            //startComms();


            
            _run = true;
        }

        private void startManagers()
        {

            foreach (ModbusManager mm in _myManagers.Values)
            {
                mm.commsSender = _sender;
            }
        }
       
            public void changeConn(object sender, PropertyChangedEventArgs e)
        {
           
            SenderConnected = _sender.senderConnected;
            ListenerConnected = _listener.listenerConnected;
            PropertyChanged?.Invoke(this, e);
        }
       
        public ConcurrentQueue<RawMove> Control { get => _control; set => _control = value; }
        public ConcurrentQueue<RawMove> Moves { get => _rawIn; set => _rawIn = value; }
        public bool ListenerConnected { get; set; } = false;
        public bool SenderConnected { get => _senderConnected; set => _senderConnected = value; }

        public void Run()
        {
            while (_run)
            {
                if (_mode == "TCP")
                {
                    //Change all this to pub/sub

                    if (Control.IsEmpty)
                    {
                        // System.Diagnostics.Debug.Write("Control is empty");

                        if (!Moves.IsEmpty)
                        {

                            // System.Diagnostics.Debug.Write("Parsing move");
                            RawMove _mv;
                            Moves.TryDequeue(out _mv);
                            if (_mv is not null)
                            {

                                if (_axes.IsEnabled(_mv.Addr))
                                {
                                    // XXX Check max values and convert (max motor speed, etc)
                                    _mv = _axes.Validate(_mv);
                                    // System.Diagnostics.Debug.WriteLine("Commsman - {0} :{1} :{2}", _mv.Addr, _mv.Reg, _mv.Val);
                                    if (_mode == "TCP")
                                    {
                                        ModbusManager.command cmd = new ModbusManager.command() { address = int.Parse(_mv.Addr), register = _mv.Reg, value = _mv.Val };

                                        String port = _axes.GetAxisConfig(_mv.Addr, "port");
                                        _myManagers[port].Command.Enqueue(cmd);
                                    }

                                }
                            }

                        }
                    }
                    else
                    {


                        RawMove _mv;
                        Control.TryDequeue(out _mv);
                        if (_mv.Addr == "240")
                        {

                            _showrunner.Heartbeat();
                        }
                        else
                        {
                            // Control messages can be for all axes - send to both ports.
                            ModbusManager.command cmd = new ModbusManager.command() { address = int.Parse(_mv.Addr), register = _mv.Reg, value = _mv.Val };
                            System.Diagnostics.Debug.WriteLine("Control message: {0} : {1} : {2}", _mv.Addr, _mv.Reg, _mv.Val);

                            if (_mv.Addr == "0")
                            {
                                System.Diagnostics.Debug.WriteLine("Control for all: {0}: {1} : {2}", cmd.address, cmd.register, cmd.value);

                                foreach (ModbusManager mm in _myManagers.Values)
                                {
                                    mm.Control.Enqueue(cmd);

                                }
                            }
                            else
                            {
                                String port = _axes.GetAxisConfig(_mv.Addr, "port");
                                _myManagers[port].Command.Enqueue(cmd);
                            }

                        }
                    }

                }
                Thread.Sleep(1);

            }
        }
        public void setMode(String mode)
        {
            if (mode != _mode)
            {

                _mode = mode;
            }
        }
              
                
            

        
        public void startSender()
        {
            if (_sender is not null)
            {
                _sender.ShutDown();
            }
            if (_mode == "TCP")
            {

                spawn_tcpsender();
            }
            else if (_mode == "UDP")
            {
                
                spawn_udpsender();
            }
            else
            {
               
                spawn_dummysender();
            }
            _sender.PropertyChanged += new PropertyChangedEventHandler(changeConn);

            Thread sendThread = new Thread(_sender.Run);
            sendThread.IsBackground = true;
            sendThread.Start();
        }
        public void startListener()
        {
            if (_listener is not null)
            {
                _listener.ShutDown();
            }
            if (_mode == "TCP")
            {

                spawn_tcplistener();
            }
            else if (_mode == "UDP")
            {
                spawn_udplistener();
            }
            else
            {
                spawn_dummylistener();
            }
            _listener.PropertyChanged += new PropertyChangedEventHandler(changeConn);
            Thread listenThread = new Thread(_listener.Run);
            listenThread.IsBackground = true;
            listenThread.Start();

        }
        private void spawn_comms()
        {
            startListener();
            startSender();
        }
       
        private void spawn_udplistener()
        {
            
            // delegates and dictionaries...
           
            _listener =  new UDPListener(_listenerAddr, _listenerPort, Moves, Control) as iListener;
            

        }
        private void spawn_udpsender()
        {
           
                try
                {
                    iSender _netsender = new UDPSender(_senderAddr, _senderPort) as iSender;
                    _sender = new SendWrapper(_netsender, _axes);


                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

            

        }
       
        private void spawn_tcplistener()
        {
            _listener = new TCPServer(_listenerAddr, _listenerPort,Moves, Control) as iListener;
            
        }
        private void spawn_tcpsender()
        {
            iSender _netsender = _listener as iSender;
            _sender = new SendWrapper(_netsender, _axes);

        }


        private void spawn_dummylistener()
        {

            // delegates and dictionaries...

            _listener = new DummyListener(Moves, Control) as iListener;


        }
        private void spawn_dummysender()
        {

            try
            {
                iSender _netsender = new DummySender() as iSender;
                _sender = new SendWrapper(_netsender, _axes);


            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }



        }

       
        public void ShutDown()
        {
            System.Diagnostics.Debug.WriteLine("closing comms");

            foreach (ModbusManager mm in _myManagers.Values)
            {
                mm.ShutDown();
            }
            _sender.ShutDown();
            _listener.ShutDown();
            _run = false;
        }
       

    }
}
