using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibNoise;

namespace WarWorldInfServer.Structures {
    public class Radar : Structure {
        public int Radius { get; set; }
        public Dictionary<Vector2Int, Structure> VisibleStructures { get; private set; }

        public Radar(Vector2Int location, string owner, int radius) : base(location, owner, StructureType.Radar) {
            Radius = radius;
            VisibleStructures = new Dictionary<Vector2Int, Structure>();
            SetVisibleStructures();
        }

        public override void OnTickUpdate() {
            base.OnTickUpdate();
            SetVisibleStructures();
        }

        public void SetVisibleStructures()
        {
            Vector2Int topLeft = new Vector2Int(Location.x - Radius, Location.y + Radius);
            Vector2Int bottomRight = new Vector2Int(Location.x + Radius, Location.y - Radius);
            SetVisibleStructures(topLeft, bottomRight);
        }

        public void SetVisibleStructures(Vector2Int topLeft, Vector2Int bottomRight) {
            Vector2Int[] structurePositions = GameServer.Instance.Structures.GetStructureLocations(topLeft, bottomRight);
            structurePositions = RadarUtility.GetVisibleObjects(new RadarUtility.RadarData(Location, Radius), structurePositions);
            VisibleStructures = GameServer.Instance.Structures.GetStructures(structurePositions);
        }
    }
}
