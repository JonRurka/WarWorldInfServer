using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace WarWorldInfinity
{
	public class UserManager
	{
		private Dictionary<string, User> _users = new Dictionary<string, User> ();
		private Dictionary<string, User> _connectedUsers = new Dictionary<string, User>();
		/* Holds the lists of users.
		 * 
		 */ 


		public UserManager ()
		{
            SaveVersions.Version_Current.User userSave = new SaveVersions.Version_Current.User("_server_", User.PermissionLevel.Server);
            AddUser(new User(userSave));
        }

        public void Frame() {
            foreach (User usr in _users.Values)
                usr.Frame();
        }

        public void TickUpdate() {
            foreach (User usr in _users.Values) {
                usr.TickUpdate();
            }
        }

		public User LoadCreateUser(string name){
			if (UserExists (name))
				return GetUser (name);
			return CreateUser (name);
		}

		public User CreateUser(string name){
			SaveVersions.Version_Current.User usrSave = new SaveVersions.Version_Current.User ();
			usrSave.name = name;
			User user = new User (usrSave);
			_users.Add (name, user);
			return user;
		}

		public User[] GetUsers(){
			return new List<User> (_users.Values).ToArray();
		}

		public string[] GetUserNames(){
			return new List<string> (_users.Keys).ToArray ();
		}

		public bool UserExists(string name){
			return _users.ContainsKey (name);
		}

		public User GetUser(string name){
			if (UserExists(name))
				return _users[name];
            Logger.LogError("User not found: " + name);
			return null;
		}

		public User GetConnectedUser(string key){
			if (SessionKeyExists (key))
				return _connectedUsers [key];
			return null;
		}

		public bool SessionKeyExists(string key){
			return _connectedUsers.ContainsKey (key);
		}

		public void AddConnectedUser(string key, User usr){
			_connectedUsers [key] = usr;
		}

		public void RemoveConnectedUser(string key){
			if (_connectedUsers.ContainsKey (key)) {
				_connectedUsers.Remove(key);
			}
		}

		public void AddUser(User user){
			if (!UserExists (user.Name)) {
                _users.Add(user.Name, user);
			}
		}

        public void LoadUsers(string folder) {
            if (Directory.Exists(folder)) {
                string[] userFolders = Directory.GetDirectories(folder);
                for (int i = 0; i < userFolders.Length; i++) {
                    string player = new DirectoryInfo(userFolders[i]).Name;
                    string savePath = folder + player + GameServer.sepChar + player + ".json";
                    if (File.Exists(savePath)) {
                        SaveVersions.Version_Current.User userSave = FileManager.LoadObject<SaveVersions.Version_Current.User>(savePath, false);
                        if (UserExists(userSave.name)) {
                            _users[userSave.name].Load(userSave);
                        }
                        else {
                            AddUser(new User(userSave));
                        }
                    }
                    else {
                        Logger.LogError("cannot find user path: " + savePath);
                    }
                }
            }
            else {
                Directory.CreateDirectory(folder);
            }
        }

        public void Save(string folder){
            try {
                if (!Directory.Exists(folder)) {
                    Directory.CreateDirectory(folder);
                }

                User[] userList = GetUsers();
                for (int i = 0; i < userList.Length; i++) {
                    SaveVersions.Version_Current.User userSave = userList[i].Save();
                    string userFoler = folder + userSave.name + GameServer.sepChar;
                    if (!Directory.Exists(userFoler))
                        Directory.CreateDirectory(userFoler);
                    FileManager.SaveConfigFile(userFoler + userSave.name + ".json", userSave, false);
                    userList[i].SaveStructures();
                }
            }
            catch(Exception e) {
                Logger.LogError("{0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
            }
			//Logger.Log ("{0} users saved.", userList.Length.ToString());
		}
	}
}

