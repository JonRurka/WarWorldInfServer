using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarWorldInfinity.Shared;

namespace WarWorldInfinity.Units {
    public class SquadController {
        private Dictionary<string, Squad> _squads;

        public SquadController() {
            _squads = new Dictionary<string, Squad>();
        }

        public void TickUpdate() {
            foreach (Squad squad in _squads.Values.ToArray())
                squad.TickUpdate();
        }

        public void AddSquad(Squad squad) {
            if (!SquadExists(squad.name))
                _squads.Add(squad.name, squad);
        }

        public void RemoveSquad(string name) {
            if (SquadExists(name))
                _squads.Remove(name);
        }

        public bool SquadExists(string name) {
            return _squads.ContainsKey(name);
        }

        public Squad[] GetSquads() {
            return _squads.Values.ToArray();
        }

        public Squad[] GetTravelingSquads(Vector2Int topLeft, Vector2Int bottomRight) {
            List<Squad> result = new List<Squad>();
            foreach (Squad squad in _squads.Values) {
                if (!squad.atStructure &&
                    squad.location.x >= topLeft.x && squad.location.x <= bottomRight.x &&
                    squad.location.y <= topLeft.y && squad.location.y >= bottomRight.y) {
                    result.Add(squad);
                }
            }
            return result.ToArray();
        }

        public Squad[] GetTravelingSquads() {
            List<Squad> result = new List<Squad>();
            foreach (Squad squad in _squads.Values) {
                if (!squad.atStructure) {
                    result.Add(squad);
                }
            }
            return result.ToArray();
        }

        public string[] GetSquadNames() {
            return _squads.Keys.ToArray();
        }
    }
}
