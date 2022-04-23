﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace UkiConsole
{
    class TCPSender : iSender
    {
        private TcpClient _tcpClient;
        // private State state = new State();
        private ConcurrentQueue<RawMove> _moveOut = new();
        private NetworkStream _networkStream;
        private bool _run = true;
        private string _addr;
        private int _port;
        public string type = "TCP";
        private bool _connected = false;
        public event PropertyChangedEventHandler PropertyChanged;
        private ConcurrentQueue<RawMove> _movein = new ConcurrentQueue<RawMove>();
        public ConcurrentQueue<RawMove> MoveIn { get => _movein; }

        public ConcurrentQueue<RawMove> MoveOut { get => _moveOut; }
        public bool senderConnected { get => _connected; }

        public TCPSender(string addr, int port)
        {

            _addr = addr;
            _port = port;
            try
            {

                _tcpClient = new TcpClient();
                //config these
                _tcpClient.Connect(_addr, _port);

                if (_tcpClient.Connected)
                {
                    _connected = true;
                    OnPropertyChanged("senderConnected");
                    _networkStream = _tcpClient.GetStream();
                    _networkStream.ReadTimeout = 2000;
                }
               
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }


        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
        public void Run()
        {
            try
            {

                _run = true;
                while (_run == true)
                {
                    while (!MoveOut.IsEmpty)
                    {
                        //System.Diagnostics.Debug.WriteLine("In TCP to unity");

                        RawMove _mv;

                        MoveOut.TryDequeue(out _mv);

                        sendStatus(_mv);
                    }


                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        public void ShutDown()
        {
            _tcpClient.Close();
            _run = false;
        }

        public void Enqueue(RawMove mv)
        {

            MoveOut.Enqueue(mv);
        }
        private void sendStatus(RawMove _mv)
        {


            // This should be a static singleton....
            byte[] data = new byte[6];
            byte[] _add = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(short.Parse(_mv.Addr)));
            byte[] _reg = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(_mv.Reg));
            byte[] _val = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(_mv.Val));
            // System.Diagnostics.Debug.WriteLine("UDP AXIS: " + _mv.Addr);
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
        public void Send(byte[] message)
        {

            if (_networkStream is null)
            {
                System.Diagnostics.Debug.WriteLine("no stream");

                _networkStream = _tcpClient.GetStream();
            }
            System.Diagnostics.Debug.WriteLine("TCP trying");
            try
            {
                System.Diagnostics.Debug.WriteLine("TCP sending");

                _networkStream.Write(message, 0, message.Length);
                _networkStream.Flush();
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("No TCP connection to send to");



                if (!_tcpClient.Connected)
                {
                    if (_connected)
                    {
                        _connected = false;
                        OnPropertyChanged("senderConnected");
                    }
                    System.Diagnostics.Debug.WriteLine("reconnecting");
                    _tcpClient = new TcpClient();
                    _tcpClient.Connect(_addr, _port);
                    _networkStream = _tcpClient.GetStream();
                    try
                    {


                        _networkStream.Write(message, 0, message.Length);
                        _networkStream.Flush();
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("Still no TCP connection to send to");

                    }
                }
                   
                if (_tcpClient.Connected)
                {
                    if (senderConnected == false)
                    {

                        OnPropertyChanged("senderConnected");
                        _connected = true;
                    }

                }
            }

        }
        
    }
}
