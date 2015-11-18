using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LibNoise;
using WarWorldInfServer.Structures;

namespace WarWorldInfServer
{
	public class User
	{
        public enum Standing {
            None,
            Own,
            Ally,
            Nuetral,
            Enemy,
        }

        public enum PermissionLevel
		{
			None,
			Observer,
			User,
			Moderator,
			Admin
		}

		public string Id { get; set; }
		public string Name { get; set; }
		public string SessionKey { get; set; }
        public string SaveFolder { get; private set; }
		public bool Connected { get; set;}
		public Stopwatch TimeSinceInteraction { get; private set;}
		public PermissionLevel Permission { get; private set;}
		public string LoginMessage {get; private set;}
        public Dictionary<Vector2Int, Structure> ownedOutposts { get; private set; }
        public Dictionary<Vector2Int, Structure> visibleOutposts { get; private set; }
        

		/* Contains all information for a user:
		 * network info (Ip, port, session key).
		 * resources
		 * outposts
		 * units (or stored in outposts).
		 * last known camera location.
		 */

		public User(){
			TimeSinceInteraction = new Stopwatch ();
            ownedOutposts = new Dictionary<Vector2Int, Structure>();
            visibleOutposts = new Dictionary<Vector2Int, Structure>();
        }

		public User (SaveVersions.Version_Current.User user) : this(){
            Deserialize (user);
		}

        public void Frame() {
            if (Connected && UserTimeout()) {
                Logout(true, "timeout");
            }
        }

        public void TickUpdate() {

        }

		public bool Login(string passwordHash){
			if (passwordHash.Equals(GameServer.Instance.DB.GetPassword(Name))) {
				SessionKey = HashHelper.RandomKey (AppSettings.SessionKeyLength);
				LoginMessage = "success";
				Connected = true;
				ResetTimer ();
				GameServer.Instance.Users.AddConnectedUser(SessionKey, this);
				return true;
			}
			SessionKey = "";
			LoginMessage = "Invalid password.";
			return false;
		}

        public void Logout(bool sendLoggout, string reason) {
            SessionKey = "";
            Connected = false;
            LoginMessage = "logged out";
            Logger.Log("{0} logged out: {1}", Name, reason);
            if (sendLoggout)
                GameServer.Instance.SockServ.Send(Name, "logout", reason);
        }

        public void SetVisibleOps() {
            foreach (Structure op in ownedOutposts.Values) {
                if (op is Radar) {
                    Radar radar = (Radar)op;
                    Structure[] VisibleStructures = radar.VisibleStructures.Values.ToArray();
                    for (int i = 0; i < VisibleStructures.Length; i++) {
                        if (!visibleOutposts.ContainsKey(VisibleStructures[i].Location)) {
                            visibleOutposts.Add(VisibleStructures[i].Location, VisibleStructures[i]);
                        }
                    }
                }
            }
        }

        public void SetFolderName() {
            SaveFolder = GameServer.Instance.AppDirectory + "Users" + GameServer.sepChar + Name + GameServer.sepChar;
        }

        public Standing GetStandings(string user) {
            if (GameServer.Instance.Users.UserExists(user))
                return GetStandings(GameServer.Instance.Users.GetUser(user));
            return Standing.None;
        }

        // TODO: test standings based on alliance.
        public Standing GetStandings(User usr) {
            if (usr.Name == Name)
                return Standing.Own;
            
            return Standing.Nuetral;
        }

        public bool CanCreateStructure(Vector2Int location, out LibNoise.SerializationStructs.MessageTypes reason) {
            if (!GameServer.Instance.Structures.CanCreateStructure(location)) {
                reason = LibNoise.SerializationStructs.MessageTypes.Invalid_Structure_Location;
                return false;
            }

            // has resources?
            reason = LibNoise.SerializationStructs.MessageTypes.None;
            return true;
        }

        public void CreateStructure(Vector2Int location, Structure.StructureType type) {
            Structure str = NewStructure(location, type);
            if (str != null) {
                if (!ownedOutposts.ContainsKey(str.Location))
                    ownedOutposts.Add(str.Location, str);
                GameServer.Instance.Structures.AddStructure(str);
            }
        }

        public bool CanUpgradeStructure(Vector2Int location, Structure.StructureType newType, out LibNoise.SerializationStructs.MessageTypes reason) {
            if (!ownedOutposts.ContainsKey(location)) {
                reason = LibNoise.SerializationStructs.MessageTypes.No_OP;
                return false;
            }
            Structure structure = ownedOutposts[location];

            if (structure.Type != Structure.StructureType.Outpost) {
                reason = LibNoise.SerializationStructs.MessageTypes.Op_Not_Upgradable;
                return false;
            }

            //check resources
            bool hasResources = true;
            if (!hasResources) {
                reason = LibNoise.SerializationStructs.MessageTypes.Not_Enough_Resources;
                return false;
            }

            //check age
            bool validAge = true;
            if (validAge) {
                reason = LibNoise.SerializationStructs.MessageTypes.Invalid_Op_Age;
                return false;
            }

            reason = LibNoise.SerializationStructs.MessageTypes.Success;
            return false;
        }

        public void changeStructure(Vector2Int location, Structure.StructureType newType) {
            if (ownedOutposts.ContainsKey(location)) {
                Structure str = NewStructure(location, newType);
                ownedOutposts[location] = str;
                GameServer.Instance.Structures.SetStructure(str);
            }
        }

        public Structure NewStructure(Vector2Int location, Structure.StructureType type) {
            Structure str = null;
            if (type != Structure.StructureType.None) {
                switch (type) {
                    case Structure.StructureType.City:
                        str = new City(location, Name);
                        break;

                    case Structure.StructureType.Outpost:
                        str = new Outpost(location, Name);
                        break;

                    case Structure.StructureType.Radar:
                        str = new Radar(location, Name, AppSettings.BaseRadarRadius);
                        break;
                }
            }
            return str;
        }

        public Structure[] GetVisibleOps(bool onlychanged) {
            SetVisibleOps();
            Structure[] visible = visibleOutposts.Values.ToArray();
            List<Structure> visibleChanged = new List<Structure>();
            if (onlychanged) {
                for (int i = 0; i < visible.Length; i++) {
                    if (GameServer.Instance.Structures.ChangedLastTick(visible[i].Location)) {
                        visibleChanged.Add(visible[i]);
                    }
                }
            }
            else
                visibleChanged.AddRange(visible);
            return visibleOutposts.Values.ToArray();
        }

        public Structure[] GetVisibleOps(Vector2Int topLeft, Vector2Int bottomRight, bool onlychanged) {
            SetVisibleOps();
            Structure[] visible = GetOps(topLeft, bottomRight, visibleOutposts);
            List<Structure> visibleChanged = new List<Structure>();
            if (onlychanged) {
                for (int i = 0; i < visible.Length; i++) {
                    if (GameServer.Instance.Structures.ChangedLastTick(visible[i].Location)) {
                        visibleChanged.Add(visible[i]);
                    }
                }
            }
            else
                visibleChanged.AddRange(visible);
            return visibleChanged.ToArray();
        }

        public Structure[] GetOwnedOps(bool onlyChanged) {
            Structure[] owned = ownedOutposts.Values.ToArray();
            List<Structure> ownedChanged = new List<Structure>();
            if (onlyChanged) {
                for (int i = 0; i < owned.Length; i++) {
                    if (GameServer.Instance.Structures.ChangedThisTick(owned[i].Location)) {
                        ownedChanged.Add(owned[i]);
                    }
                }
            }
            else
                ownedChanged.AddRange(owned);

            return ownedChanged.ToArray();
        }
        
        public Structure[] GetOwnedOps(Vector2Int topLeft, Vector2Int bottomRight, bool onlyChanged) {
            Structure[] owned = GetOps(topLeft, bottomRight, ownedOutposts);
            List <Structure> ownedChanged = new List<Structure>();
            if (onlyChanged) {
                for (int i = 0; i < owned.Length; i++) {
                    if (GameServer.Instance.Structures.ChangedThisTick(owned[i].Location)) {
                        ownedChanged.Add(owned[i]);
                    }
                }
            }
            else
                ownedChanged.AddRange(owned);

            return ownedChanged.ToArray();
        }

        public Structure[] GetOps(Vector2Int topLeft, Vector2Int bottomRight, Dictionary<Vector2Int, Structure> dict) {
            List<Structure> result = new List<Structure>();
            foreach (Vector2Int position in dict.Keys) {
                if (position.x >= topLeft.x && position.x <= bottomRight.x &&
                    position.y <= topLeft.y && position.y >= bottomRight.y) {
                    result.Add(dict[position]);
                }
            }
            return result.ToArray();
        }

        public SaveVersions.Version_Current.User GetSerializer(){
			SaveVersions.Version_Current.User user = new SaveVersions.Version_Current.User ();
			user.name = Name;
			user.permission = Permission;
			return user;
		}

		public void Deserialize(SaveVersions.Version_Current.User user){
			Name = user.name;
            string permissionStr = GameServer.Instance.DB.GetPermission(Name);
            Permission = (PermissionLevel)Enum.Parse(typeof(PermissionLevel), permissionStr, true);
            SetFolderName();
            // TODO: load users.

        }

		public void ResetTimer(){
			TimeSinceInteraction.Reset ();
			TimeSinceInteraction.Start ();
		}

		public bool UserTimeout(){
			return TimeSinceInteraction.Elapsed.Seconds >= AppSettings.UserTimeoutTime;
		}
	}
}

