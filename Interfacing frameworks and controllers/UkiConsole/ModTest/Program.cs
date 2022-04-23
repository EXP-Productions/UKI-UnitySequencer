using System;
using System.Collections.Generic;
using UkiConsole;

using System.Threading;

namespace ModTest
{
    class Program
    {
        static void Main(string[] args)
        {

            List<int> ess = new List<int> { (int)ModMap.RegMap.MB_GOTO_POSITION };
            List<String> axes = new List<String> { "18", };

            ModbusManager _myManager = new ModbusManager("COM8", axes, ess, 19200);
            Console.WriteLine("Got manager");

            Thread manThread = new Thread(_myManager.Listen);
            Console.WriteLine("Listening");

            manThread.Start();
            Console.WriteLine("Started");
            foreach (String ax in axes)
            {

               // System.Diagnostics.Debug.WriteLine("Sending command: {0} : {1}: {2}", ax, 30, 30);

               // ModbusManager.command mv = new ModbusManager.command() { address = int.Parse(ax), register = (int)ModMap.RegMap.MB_GOTO_POSITION, value = 30 };
               // _myManager.Command.Enqueue(mv);
            }

        }
    }
}
