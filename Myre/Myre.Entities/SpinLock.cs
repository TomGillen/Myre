using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Myre.Entities
{
    struct SpinLock
    {
        private int locked;

        public void Lock()
        {
            while (Interlocked.CompareExchange(ref locked, 1, 0) != 0)
                Thread.Sleep(0);
        }

        public void Unlock()
        {
            Interlocked.CompareExchange(ref locked, 0, 1);
        }
    }
}
