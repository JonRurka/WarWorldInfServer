using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarWorldInfinity.Shared;
using WarWorldInfinity.Shared.Structures;

namespace WarWorldInfinity.Structures {
    public class Structure {
        public enum StructureType {
            None,
            Outpost,
            City,
            Radar,
        }
        public struct StructureSave {
            public Vector2Int location;
            public StructureType type;
            public int activeTicks;
            public IExtraData extraData;

            public StructureSave(Vector2Int location, StructureType type, int activeTicks, IExtraData extraData) {
                this.location = location;
                this.type = type;
                this.activeTicks = activeTicks;
                this.extraData = extraData;
            }
        }
        public delegate void Command(string args);

        public Vector2Int Location { get; private set; }
        public User Owner { get; set; }
        public bool Enabled { get; set; }
        public int ActiveTicks { get; private set; }
        public StructureType Type { get; private set; }
        public Alliance alliance { get; private set; }
        public IExtraData extraData { get; protected set; }

        private Dictionary<string, Command> _commands;

        public Structure(Vector2Int location, User owner, StructureType type) {
            _commands = new Dictionary<string, Command>();
            Enabled = true;
            Location = location;
            Owner = owner;
            Type = type;
        }

        public virtual void TickUpdate() {
            ActiveTicks++;
        }

        public virtual void Destroyed() {
            Enabled = false;
        }

        public virtual void SetOwner(User newOwner) {
            Owner = newOwner;
            alliance = Owner.alliance;
        }

        public virtual StructureSave Save() {
            return new StructureSave(Location, Type, ActiveTicks, extraData);
        }

        public virtual void Load(StructureSave structureSave) {
            Location = structureSave.location;
            ActiveTicks = structureSave.activeTicks;
            alliance = Owner.alliance;
            extraData = structureSave.extraData;
        }

        public void CallCommand(string cmd, string args) {
            if (_commands.ContainsKey(cmd.ToLower())) {
                _commands[cmd.ToLower()](args);
            }
            else
                Logger.LogError("Invalid structure command: " + cmd);
        }

        protected void AddCommand(string cmd, Command callback) {
            if (!_commands.ContainsKey(cmd)) {
                _commands.Add(cmd, callback);
            }
        }

        protected void RemoveCommand(string cmd) {
            if (_commands.ContainsKey(cmd))
                _commands.Remove(cmd);
        }
    }
}
