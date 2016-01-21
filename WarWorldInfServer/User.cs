using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using shared = WarWorldInfinity.Shared;
using WarWorldInfinity.Structures;
using WarWorldInfinity.Units;

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

        public struct ModeratorAction {
            public enum Type {
                Kick,
                TempMute,
                PermaMute,
                Ban,
                PermaBan
            }
            public Type actionType;
            public int ticksRemaining;
            public int tick;
            public string timeStamp;
            public string reason;
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
        public Dictionary<shared.Vector2Int, Structure> ownedOutposts { get; private set; }
        public Dictionary<shared.Vector2Int, Structure> visibleOutposts { get; private set; }
        public Dictionary<string, Squad> squads { get; private set; }
        public Dictionary<string, Squad> visibleSquads { get; private set; }
        public List<ModeratorAction> ModActions { get; private set; }

		/* Contains all information for a user:
		 * network info (Ip, port, session key).
		 * resources
		 * outposts
		 * units (or stored in outposts).
		 * last known camera location.
		 */

		public User(){
			TimeSinceInteraction = new Stopwatch ();
            ownedOutposts = new Dictionary<shared.Vector2Int, Structure>();
            visibleOutposts = new Dictionary<shared.Vector2Int, Structure>();
            squads = new Dictionary<string, Squad>();
            visibleSquads = new Dictionary<string, Squad>();
            ModActions = new List<ModeratorAction>();
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
                //Logger.Log("structures: " + strList.Length);
                for (int i = 0; i < strList.Length; i++) {
                    if (!visibleOutposts.ContainsKey(strList[i].Location) && strList[i].Owner.Name != Name)
                        visibleOutposts.Add(strList[i].Location, strList[i]);
                }
                //Logger.Log("Visible ops: " + visibleOutposts.Count);
            }
            else {
                foreach (Structure op in ownedOutposts.Values) {
                    if (op is Radar) {
                        Radar radar = (Radar)op;
                        radar.SetVisibleStructures();
                        shared.Vector2Int[] VisibleStructures = radar.GetVisibleStructures();
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
                    //Logger.Log("{0}: {1} radar visible ops.", Name, visibleOutposts.Count);
                }
                //Logger.Log("{0}: {1} visible ops.", Name, visibleOutposts.Count);
            }
        }

        public void UpdateSquads() {
            foreach (Structure str in ownedOutposts.Values.ToArray()) {
                UpdateSquads(str);
            }
        }

        public void UpdateSquads(Structure str) {
            Squad[] sq = str.Squads.ToArray();
            for (int i = 0; i < sq.Length; i++) {
                if (!squads.ContainsKey(sq[i].ident))
                    squads.Add(sq[i].ident, sq[i]);
                else
                    squads[sq[i].ident] = sq[i];
            }
        }

        public void SetVisibleSquads() {
            if (Permission == PermissionLevel.Server || Permission == PermissionLevel.Admin) {
                Squad[] squadList = GameServer.Instance.Squads.GetSquads();
                for (int i = 0; i < squadList.Length; i++) {
                    if (visibleSquads.ContainsKey(squadList[i].name) && squadList[i].owner.Name != Name)
                        visibleSquads.Add(squadList[i].name, squadList[i]);
                }
            }
            else {
                
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

        public Standing GetStandings(User usr) {
            if (usr.Permission == PermissionLevel.Admin || usr.Permission == PermissionLevel.Admin)
                return Standing.Own;
            if (usr.Name == Name)
                return Standing.Own;
            if (alliance != null && alliance.HasMember(usr))
                return Standing.Ally;
            if (alliance != null && usr.alliance != null && alliance.IsEnemy(usr.alliance.Name))
                return Standing.Enemy;

            return Standing.Nuetral;
        }

        public void SendCommand(shared.Vector2Int location, string command, string args) {
            if (ownedOutposts.ContainsKey(location)) {
                ownedOutposts[location].CallCommand(command, args);
            }
            else
                Logger.LogError("No structure at " + location);
        }

        public bool CanCreateStructure(shared.Vector2Int location, out shared.MessageTypes reason) {
            if (!GameServer.Instance.Structures.CanCreateStructure(location)) {
                reason = shared.MessageTypes.Invalid_Structure_Location;
                return false;
            }

            // has resources?
            reason = shared.MessageTypes.None;
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

        public Structure CreateStructure(shared.Vector2Int location, Structure.StructureType type, bool updateImmediately) {
            Structure str = NewStructure(location, type);
            if (str != null) {
                if (!ownedOutposts.ContainsKey(str.Location))
                    ownedOutposts.Add(str.Location, str);
                GameServer.Instance.Structures.AddStructure(str, updateImmediately);
            }
            return str;
        }

        public bool CanUpgradeStructure(shared.Vector2Int location, Structure.StructureType newType, out shared.MessageTypes reason) {
            if (!ownedOutposts.ContainsKey(location)) {
                reason = shared.MessageTypes.No_OP;
                return false;
            }
            Structure structure = ownedOutposts[location];

            if (structure.Type != Structure.StructureType.Outpost) {
                reason = shared.MessageTypes.Op_Not_Upgradable;
                return false;
            }

            //check resources
            bool hasResources = true;
            if (!hasResources) {
                reason = shared.MessageTypes.Not_Enough_Resources;
                return false;
            }

            //check age
            bool validAge = true;
            if (!validAge) {
                reason = shared.MessageTypes.Invalid_Op_Age;
                return false;
            }

            reason = shared.MessageTypes.Success;
            return true;
        }

        public void changeStructure(shared.Vector2Int location, Structure.StructureType newType) {
            if (ownedOutposts.ContainsKey(location)) {
                Structure str = NewStructure(location, newType);
                ownedOutposts[location] = str;
                GameServer.Instance.Structures.SetStructure(str);
            }
        }

        public Structure NewStructure(shared.Vector2Int location, Structure.StructureType type) {
            Structure str = null;
            switch (type) {
                case Structure.StructureType.None:
                    str = new None(location, this);
                    break;

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

        public Structure[] GetVisibleOps(shared.Vector2Int topLeft, shared.Vector2Int bottomRight, bool onlychanged) {
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
        
        public Structure[] GetOwnedOps(shared.Vector2Int topLeft, shared.Vector2Int bottomRight, bool onlyChanged) {
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

        public Structure[] GetOps(shared.Vector2Int topLeft, shared.Vector2Int bottomRight, Dictionary<shared.Vector2Int, Structure> dict) {
            List<Structure> result = new List<Structure>();
            foreach (shared.Vector2Int position in dict.Keys) {
                if (position.x >= topLeft.x && position.x <= bottomRight.x &&
                    position.y <= topLeft.y && position.y >= bottomRight.y) {
                    result.Add(dict[position]);
                }
            }
            return result.ToArray();
        }

        public bool HasCity() {
            foreach(Structure str in ownedOutposts.Values) {
                if (str.Type == Structure.StructureType.City)
                    return true;
            }
            return false;
        }

        public SaveVersions.Version_Current.User Save(){
			SaveVersions.Version_Current.User user = new SaveVersions.Version_Current.User ();
			user.name = Name;
			user.permission = Permission;
            SaveSquads();
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
            LoadSquads();
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

        public void LoadSquads() {
            if (GameServer.Instance.WorldLoaded) {
                string file = SaveFolder + "Units.json";
                if (File.Exists(file)) {
                    Squad.SquadSave[] saves = FileManager.LoadObject<Squad.SquadSave[]>(file, false);
                    for (int i = 0; i < saves.Length; i++) {
                        Squad unit = new Squad(this, saves[i]);
                        squads.Add(unit.ident, unit);
                        if (unit.atStructure)
                            GameServer.Instance.Structures.GetStructure(unit.location).Squads.Add(unit);
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

        public void SaveSquads() {
            if (GameServer.Instance.WorldLoaded) {
                string file = SaveFolder + "Units.json";
                if (!File.Exists(file))
                    File.Create(file).Close();
                List<Squad.SquadSave> saves = new List<Squad.SquadSave>();
                foreach (Squad sqd in squads.Values) {
                    saves.Add(new Squad.SquadSave(sqd.name, sqd.ident, sqd.location, sqd.data, sqd.atStructure));
                }
                FileManager.SaveConfigFile(file, saves.ToArray(), false);
            }
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

