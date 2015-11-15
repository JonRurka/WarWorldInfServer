using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibNoise;

namespace WarWorldInfServer.Structures {
    public class Structure {
        public enum StructureType {
            None,
            Outpost,
            City,
            Radar,
        }
        public delegate void Command(string args);

        public Vector2Int Location { get; private set; }
        public string Owner { get; private set; }
        public bool Enabled { get; private set; }
        public int ActiveTicks { get; private set; }
        public StructureType Type { get; private set; }

        private Dictionary<string, Command> _commands;

        public Structure(Vector2Int location, string owner, StructureType type) {
            _commands = new Dictionary<string, Command>();
            Enabled = true;
            Location = location;
            Owner = owner;
            Type = type;
        }

        public virtual void OnTickUpdate() {
            ActiveTicks++;
        }

        public virtual void OnDestroyed() {
            Enabled = false;
        }

        public virtual void OnSetOwner(string newOwner) {
            Owner = newOwner;
        }

        public void CallCommand(string cmd, string args) {
            if (_commands.ContainsKey(cmd))
                _commands[cmd](args);
        }

        protected void AddCommand(string cmd, Command callback) {
            if (!_commands.ContainsKey(cmd))
                _commands.Add(cmd, callback);
        }

        protected void RemoveCommand(string cmd) {
            if (_commands.ContainsKey(cmd))
                _commands.Remove(cmd);
        }
    }
}
