using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarWorldInfinity.Shared;
using WarWorldInfinity.LibNoise;

namespace WarWorldInfinity {
    public class Alliance {
        public struct AllianceSave {
            public string name;
            public string owner;
            public string[] members;

            public AllianceSave(string name, string owner, string[] members) {
                this.name = name;
                this.owner = owner;
                this.members = members;
            }
        }

        public class Member {
            public User user;

            public Member(User user) {
                this.user = user;
            }
        }

        public string Name { get; set; }
        public User Owner { get; set; }
        public List<Member> members;
        public List<Vector2Int> structures;

        public Alliance(string name, User owner) {
            Name = name;
            Owner = owner;
            members = new List<Member>();
        }

        public Alliance(AllianceSave save) {
            members = new List<Member>();
            Load(save);
        }

        public void UpdateTick() {
            
        }

        public void Load(AllianceSave save) {
            if (GameServer.Instance.Users.UserExists(save.owner)) {
                members.Clear();
                Name = save.name;
                Owner = GameServer.Instance.Users.GetUser(save.owner);
                for (int i = 0; i < save.members.Length; i++) {
                    if (GameServer.Instance.Users.UserExists(save.members[i])) {
                        User user = GameServer.Instance.Users.GetUser(save.members[i]);
                        user.alliance = this;
                        members.Add(new Member(user));
                    }
                }
            }
            
        }

        public AllianceSave Save() {
            List<string> memberNames = new List<string>();
            for (int i = 0; i < members.Count; i++) {
                memberNames.Add(members[i].user.Name);
            }
            return new AllianceSave(Name, Owner.Name, memberNames.ToArray());
        }
    }
}
