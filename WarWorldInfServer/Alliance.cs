using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarWorldInfinity.Shared;
using WarWorldInfinity.LibNoise;

namespace WarWorldInfinity {
    public class Alliance {
        public class Member {
            public User user;
        }

        public string Name { get; set; }
        public User Owner { get; set; }
        public List<Member> members;
        public List<Vector2Int> structures;

        public Alliance() {
            
        }

        public void UpdateTick() {
            
        }

        public void CreateNew(string name, User owner) {
            Name = name;
            Owner = owner;

            Save("path/to/file");
        }

        public void Load(string file) {
            
        }

        public void Save(string file) {
            
        }
    }
}
