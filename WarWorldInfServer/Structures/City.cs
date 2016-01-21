using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarWorldInfinity.Shared;
using WarWorldInfinity.Units;

namespace WarWorldInfinity.Structures {
    public class City : Structure {
        public City(Vector2Int location, User owner) : base(location, owner, StructureType.City) {
            AddCommand("createsquad", CreateSquad);
        }

        public void CreateSquad(string name) {
            AirSquad squad = new AirSquad(Owner, name, this);
            AddSquad(squad);
            Owner.UpdateSquads(this);
        }
    }
}
