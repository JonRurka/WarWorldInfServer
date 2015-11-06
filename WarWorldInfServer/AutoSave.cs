using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace WarWorldInfServer
{
    public class AutoSave
    {
        private Timer _timer;

        public AutoSave()
        {
        }

        public void Start(Action callback, int secondInterval)
        {
            Stop();
            _timer = new Timer((e) => callback(), null, TimeSpan.Zero, TimeSpan.FromSeconds(secondInterval));
        }

        public void Stop()
        {
            if (_timer != null)
                _timer.Dispose();
        }

    }
}
