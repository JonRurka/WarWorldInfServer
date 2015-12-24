using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using lib = WarWorldInfinity.Shared;
using WarWorldInfinity.Structures;

namespace WarWorldInfinity
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
			Admin,
            Server
		}

		public string Id { get; set; }
		public string Name { get; set; }
        public Alliance alliance { get; set; }
		public string SessionKey { get; set; }
        public string SaveFolder { get; private set; }
		public bool Connected { get; set;}
		public Stopwatch TimeSinceInteraction { get; private set;}
		public PermissionLevel Permission { get; private set;}
		public string LoginMessage {get; private set;}
        public Dictionary<lib.Vector2Int, Structure> ownedOutposts { get; private set; }
        public Dictionary<lib.Vector2Int, Structure> visibleOutposts { get; private set; }
        

		/* Contains all information for a user:
		 * network info (Ip, port, session key).
		 * resources
		 * outposts
		 * units (or stored in outposts).
		 * last known camera location.
		 */

		public User(){
			TimeSinceInteraction = new Stopwatch ();
            ownedOutposts = new Dictionary<lib.Vector2Int, Structure>();
            visibleOutposts = new Dictionary<lib.Vector2Int, Structure>();
        }

		public User (SaveVersions.Version_Current.User user) : this(){
            Load (user);
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
            if (Permission == PermissionLevel.Server || Permission == PermissionLevel.Admin) {
                Structure[] strList = GameServer.Instance.Structures.GetStructures();
                Logger.Log("structures: " + strList.Length);
                for (int i = 0; i < strList.Length; i++) {
                    if (!visibleOutposts.ContainsKey(strList[i].Location) && strList[i].Owner.Name != Name)
                        visibleOutposts.Add(strList[i].Location, strList[i]);
                }
                Logger.Log("Visible ops: " + visibleOutposts.Count);
            }
            else {
                foreach (Structure op in ownedOutposts.Values) {
                    if (op is Radar) {
                        Radar radar = (Radar)op;
                        radar.SetVisibleStructures();
                        lib.Vector2Int[] VisibleStructures = radar.GetVisibleStructures();
                        for (int i = 0; i < VisibleStructures.Length; i++) {
                            Structure structure = GameServer.Instance.Structures.GetStructure(VisibleStructures[i]);
                            if (structure != null) {
                                if (!visibleOutposts.ContainsKey(VisibleStructures[i]) && structure.Owner.Name != Name) {
                                    visibleOutposts.Add(VisibleStructures[i], structure);
                                }
                            }
                            else
                                Logger.LogError("Cannot find structure: " + VisibleStructures[i]);
                        }
                    }
                    Logger.Log("{0}: {1} radar visible ops.", Name, visibleOutposts.Count);
                }
                Logger.Log("{0}: {1} visible ops.", Name, visibleOutposts.Count);
            }
        }

        public void SetFolderName() {
            if (GameServer.Instance.WorldLoaded)
                SaveFolder = GameServer.Instance.Worlds.CurrentWorldDirectory + "Users" + GameServer.sepChar + Name + GameServer.sepChar;
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

        public void SendCommand(lib.Vector2Int location, string command) {
            if (ownedOutposts.ContainsKey(location)) {
                ownedOutposts[location].CallCommand(command, "");
            }
            else
                Logger.LogError("No structure at " + location);
        }

        public bool CanCreateStructure(lib.Vector2Int location, out lib.MessageTypes reason) {
            if (!GameServer.Instance.Structures.CanCreateStructure(location)) {
                reason = lib.MessageTypes.Invalid_Structure_Location;
                return false;
            }

            // has resources?
            reason = lib.MessageTypes.None;
            return true;
        }

        public bool CanCallCommand(string command) {
            if (GameServer.Instance.CommandExec.CommandExists(command)) {
                CommandDescription desc = GameServer.Instance.CommandExec.GetCommandDescription(command);
                switch (Permission) {
                    case PermissionLevel.Server:
                        return true;
                    case PermissionLevel.Admin:
                        return (desc.permission == PermissionLevel.Admin || 
                                desc.permission == PermissionLevel.Moderator || 
                                desc.permission == PermissionLevel.User);
                    case PermissionLevel.Moderator:
                        return (desc.permission == PermissionLevel.Moderator || 
                                desc.permission == PermissionLevel.User);
                    case PermissionLevel.User:
                        return desc.permission == PermissionLevel.User;
                    default:
                        return false;
                }
            }
            return false;
        }

        public Structure CreateStructure(lib.Vector2Int location, Structures.Structure.StructureType type, bool updateImmediately) {
            Structure str = NewStructure(location, type);
            if (str != null) {
                if (!ownedOutposts.ContainsKey(str.Location))
                    ownedOutposts.Add(str.Location, str);
                GameServer.Instance.Structures.AddStructure(str, updateImmediately);
            }
            return str;
        }

        public bool CanUpgradeStructure(lib.Vector2Int location, Structures.Structure.StructureType newType, out lib.MessageTypes reason) {
            if (!ownedOutposts.ContainsKey(location)) {
                reason = lib.MessageTypes.No_OP;
                return false;
            }
            Structure structure = ownedOutposts[location];

            if (structure.Type != Structure.StructureType.Outpost) {
                reason = lib.MessageTypes.Op_Not_Upgradable;
                return false;
            }

            //check resources
            bool hasResources = true;
            if (!hasResources) {
                reason = lib.MessageTypes.Not_Enough_Resources;
                return false;
            }

            //check age
            bool validAge = true;
            if (!validAge) {
                reason = lib.MessageTypes.Invalid_Op_Age;
                return false;
            }

            reason = lib.MessageTypes.Success;
            return true;
        }

        public void changeStructure(lib.Vector2Int location, Structures.Structure.StructureType newType) {
            if (ownedOutposts.ContainsKey(location)) {
                Structure str = NewStructure(location, newType);
                ownedOutposts[location] = str;
                GameServer.Instance.Structures.SetStructure(str);
            }
        }

        public Structure NewStructure(lib.Vector2Int location, Structures.Structure.StructureType type) {
            Structure str = null;
            switch (type) {
                case Structure.StructureType.City:
                    str = new City(location, this);
                    break;

                case Structure.StructureType.Outpost:
                    str = new Outpost(location, this);
                    break;

                case Structure.StructureType.Radar:
                    str = new Radar(location, this, AppSettings.BaseRadarRadius);
                    break;
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

        public Structure[] GetVisibleOps(lib.Vector2Int topLeft, lib.Vector2Int bottomRight, bool onlychanged) {
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
        
        public Structure[] GetOwnedOps(lib.Vector2Int topLeft, lib.Vector2Int bottomRight, bool onlyChanged) {
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

        public Structure[] GetOps(lib.Vector2Int topLeft, lib.Vector2Int bottomRight, Dictionary<lib.Vector2Int, Structure> dict) {
            List<Structure> result = new List<Structure>();
            foreach (lib.Vector2Int position in dict.Keys) {
                if (position.x >= topLeft.x && position.x <= bottomRight.x &&
                    position.y <= topLeft.y && position.y >= bottomRight.y) {
                    result.Add(dict[position]);
                }
            }
            return result.ToArray();
        }

        public SaveVersions.Version_Current.User Save(){
			SaveVersions.Version_Current.User user = new SaveVersions.Version_Current.User ();
			user.name = Name;
			user.permission = Permission;
			return user;
		}

		public void Load(SaveVersions.Version_Current.User user){
			Name = user.name;
            if (user.permission != PermissionLevel.Server) {
                string permissionStr = GameServer.Instance.DB.GetPermission(Name);
                Permission = (PermissionLevel)Enum.Parse(typeof(PermissionLevel), permissionStr, true);
            }
            else
                Permission = PermissionLevel.Server;
            SetFolderName();
            LoadStructures();
        }

        public void LoadStructures() {
            if (GameServer.Instance.WorldLoaded) {
                string file = SaveFolder + "structures.json";
                if (File.Exists(file)) {
                    Structure.StructureSave[] saves = FileManager.LoadObject<Structure.StructureSave[]>(file, true);
                    for (int i = 0; i < saves.Length; i++) {
                        Structure str = CreateStructure(saves[i].location, saves[i].type, true);
                        if (str != null)
                            str.Load(saves[i]);
                        else
                            Logger.Log("Failed to load structure.");
                    }
                }
            }
        }

        public void SaveStructures() {
            List<Structure.StructureSave> saves = new List<Structure.StructureSave>();
            foreach (Structure op in ownedOutposts.Values) {
                saves.Add(op.Save());
            }
            FileManager.SaveConfigFile(SaveFolder + "structures.json", saves.ToArray(), true);
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

