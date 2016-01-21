using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarWorldInfinity.Shared;
using Structure = WarWorldInfinity.Structures.Structure;

namespace WarWorldInfinity.Units {
    public class AirSquad : Squad {

        public AirSquad(User owner, string name, Structure structure) : base (owner, name, structure) {

        }

        public AirSquad(User owner, SquadSave save) : base (owner, save) {
            
        }

        public override void TickUpdate() {
            base.TickUpdate();
        }

        public override void Deploy(Vector2Int start, Vector2Int finish) {
            // check for resources.
            base.Deploy(start, finish);
        }
    }
}
