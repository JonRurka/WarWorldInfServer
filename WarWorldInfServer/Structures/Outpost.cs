using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibNoise;

namespace WarWorldInfServer.Structures {
    class Outpost : Structure {
        public Outpost(Vector2Int location, string owner) : base(location, owner, StructureType.Outpost) {
            AddCommand("test", Test_CMD);
        }

        public override void OnDestroyed() {
            base.OnDestroyed();
        }

        public override void OnTickUpdate() {
            base.OnTickUpdate();
        }

        private void Test_CMD(string args) {
            Logger.Log(args);
        }
    }
}
