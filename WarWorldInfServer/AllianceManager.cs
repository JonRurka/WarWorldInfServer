using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarWorldInfinity {
    public class AllianceManager {
        private Dictionary<string, Alliance> _alliances;

        public AllianceManager() {
        }

        public void UpdateTick() {
            
        }

        public void CreateAlliance(string name, User owner) {
            // see if can create alliance.
            if (!AllianceExist(name)) {
                Alliance all = new Alliance();
                all.CreateNew(name, owner);
                _alliances.Add(name, all);
            }
        }

        public Alliance GetAlliance(string name) {
            if (AllianceExist(name))
                return _alliances[name];
            return null;
        }

        public bool AllianceExist(string alliance) {
            return _alliances.ContainsKey(alliance);
        }

        public void Save(string file) {
            
        }

        public void Load(string file) {
            
        }
    }
}
