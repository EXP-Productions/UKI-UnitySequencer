using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.ComponentModel;


namespace UkiConsole
{
    public interface iSender
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool senderConnected { get; }
        public ConcurrentQueue<RawMove> MoveIn { get; }

        public void Run();
        public void ShutDown();
        public void Enqueue(RawMove _mv);
    }
}
