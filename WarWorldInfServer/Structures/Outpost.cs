using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarWorldInfinity.Shared;

namespace WarWorldInfinity.Structures {
    class Outpost : Structure {
        public Outpost(Vector2Int location, User owner) : base(location, owner, StructureType.Outpost) {
            AddCommand("test", Test_CMD);
        }

        public override void Destroy() {
            base.Destroy();
        }

        public override void TickUpdate() {
            base.TickUpdate();
        }

        private void Test_CMD(string args) {
            Logger.Log("It Worked woo!");
        }
    }
}
