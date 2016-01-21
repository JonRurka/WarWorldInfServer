using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarWorldInfinity.Shared;

namespace WarWorldInfinity {
    public class Alliance {
        public struct AllianceSave {
            public string name;
            public string owner;
            public string[] members;
            public string[] enemies;

            public AllianceSave(string name, string owner, string[] members, string[] enemies) {
                this.name = name;
                this.owner = owner;
                this.members = members;
                this.enemies = enemies;
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
        public Dictionary<string, Member> members;
        public List<Vector2Int> structures;
        public Dictionary<string, Alliance> enemies;

        public Alliance(string name, User owner) {
            Name = name;
            Owner = owner;
            members = new Dictionary<string, Member>();
        }

        public Alliance(AllianceSave save) {
            members = new Dictionary<string, Member>();
            Load(save);
        }

        public void UpdateTick() {
            
        }

        public bool HasMember(User user) {
            return members.ContainsKey(user.Name);
        }

        public bool HasMember(string user) {
            return members.ContainsKey(user);
        }

        public void AddMember(User user) {
            if (!HasMember(user)) {
                members.Add(user.Name, new Member(user));
            }
        }

        public bool IsEnemy(string alliance) {
            return enemies.ContainsKey(alliance);
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
                        members.Add(user.Name, new Member(user));
                    }
                }
            }
        }

        public AllianceSave Save() {
            List<string> memberNames = new List<string>(members.Keys.ToArray());
            return new AllianceSave(Name, Owner.Name, memberNames.ToArray(), enemies.Keys.ToArray());
        }
    }
}
