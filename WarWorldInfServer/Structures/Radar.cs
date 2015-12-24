using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarWorldInfinity.Shared;
using WarWorldInfinity.Shared.Structures;

namespace WarWorldInfinity.Structures {
    public class Radar : Structure {

        public int Radius { get; set; }
        public List<Vector2Int> VisibleStructures { get; private set; }

        public Radar(Vector2Int location, User owner, int radius) : base(location, owner, StructureType.Radar) {
            Radius = radius;
            extraData = new RadarData(Radius);
            VisibleStructures = new List<Vector2Int>();
        }

        public override void TickUpdate() {
            base.TickUpdate();
            SetVisibleStructures();
        }

        public override StructureSave Save() {
            extraData = new RadarData(Radius);
            return base.Save();
        }

        public override void Load(StructureSave structureSave) {
            base.Load(structureSave);
            Radius = ((RadarData)extraData).radius;
        }

        public void SetVisibleStructures()
        {
            Logger.Log("setting visible structures");
            Vector2Int topLeft = new Vector2Int(Location.x - Radius, Location.y + Radius);
            Vector2Int bottomRight = new Vector2Int(Location.x + Radius, Location.y - Radius);
            SetVisibleStructures(topLeft, bottomRight);
        }

        public void SetVisibleStructures(Vector2Int topLeft, Vector2Int bottomRight) {
            Vector2Int[] structurePositions = GameServer.Instance.Structures.GetStructureLocations(topLeft, bottomRight);
            Logger.Log("structures in square: " + structurePositions.Length);
            //VisibleStructures = new List<Vector2Int>(RadarUtility.GetVisibleObjects(new RadarUtility.RadarData(Location, Radius), structurePositions));
            VisibleStructures = new List<Vector2Int>(GetVisibleLocations(structurePositions));
            Logger.Log("Structures found: " + VisibleStructures.Count);
        }

        public bool IsVisible(Vector2Int location) {
            return VisibleStructures.Contains(location);
        }

        public Vector2Int[] GetVisibleStructures() {
            return VisibleStructures.ToArray();
        }

        private Vector2Int[] GetVisibleLocations(Vector2Int[] locations) {
            List<Vector2Int> result = new List<Vector2Int>();
            for (int i = 0; i < locations.Length; i++) {
                if (Vector2Int.Distance(Location, locations[i]) <= Radius)
                    result.Add(locations[i]);
            }
            return result.ToArray();
        }
    }
}
