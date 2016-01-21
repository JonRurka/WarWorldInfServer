using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarWorldInfinity.Shared;

namespace WarWorldInfinity.Structures {
    public class None : Structure {
        public None(Vector2Int location, User owner) : base(location, owner, StructureType.Outpost) {
            
        }
    }
}
