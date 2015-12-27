using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarWorldInfinity {
    public class AllianceManager {
        private Dictionary<string, Alliance> _alliances;

        public AllianceManager() {
            _alliances = new Dictionary<string, Alliance>();
        }

        public void UpdateTick() {
            
        }

        public bool CreateAlliance(string name, string owner) {
            if (GameServer.Instance.Users.UserExists(owner)) {
                return CreateAlliance(name, GameServer.Instance.Users.GetUser(owner));
            }
            return false;
        }
        
        public bool CreateAlliance(string name, User owner) {
            // see if can create alliance.
            if (!AllianceExist(name)) {
                Alliance all = new Alliance(name, owner);
                _alliances.Add(name, all);
                return true;
            }
            return false;
        }

        public Alliance GetAlliance(string name) {
            if (AllianceExist(name))
                return _alliances[name];
            return null;
        }

        public bool AllianceExist(string alliance) {
            return _alliances.ContainsKey(alliance);
        }

        public Alliance.AllianceSave[] Save() {
            List<Alliance.AllianceSave> saves = new List<Alliance.AllianceSave>();
            foreach (Alliance all in _alliances.Values)
                saves.Add(all.Save());
            return saves.ToArray();
        }

        public void Load(Alliance.AllianceSave[] saves) {
            _alliances.Clear();
            for (int i = 0; i < saves.Length; i++) {
                _alliances.Add(saves[i].name, new Alliance(saves[i]));
            }
        }
    }
}
