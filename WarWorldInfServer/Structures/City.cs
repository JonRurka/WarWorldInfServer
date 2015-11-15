using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibNoise;

namespace WarWorldInfServer.Structures {
    public class City : Structure {
        public City(Vector2Int location, string owner) : base(location, owner, StructureType.City) {
            
        }
    }
}
