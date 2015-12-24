using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WarWorldInfinity.Shared.Structures {
    public struct RadarData : IExtraData {
        public int radius;

        public RadarData(int radius) {
            this.radius = radius;
        }
    }
}
