using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace UkiConsole
{
    interface iListener
    {
        public bool listenerConnected { get; }
        public event PropertyChangedEventHandler PropertyChanged;
        public string Type { get; }

        public void Run();
        public void ShutDown();
    }
}
