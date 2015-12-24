using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarWorldInfinity {
    public class FrameCounter {
        private Stopwatch _watch;
        private int _frames;

        public int FPS { get; private set; }

        public FrameCounter() {
            _frames = 0;
            _watch = new Stopwatch();
            _watch.Start();
        }

        public void Update() {
            _frames++;
            long milliseconds = _watch.ElapsedMilliseconds;
            if (milliseconds % 1000 == 0) {
                FPS = _frames;
                _frames = 0;
                Console.Title = "FPS: " + FPS;
                //Logger.Log("FPS: " + FPS);
            }
        }
    }
}
