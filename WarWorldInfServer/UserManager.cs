using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace WarWorldInfServer
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

		public void LoadUsers(string folder){
			if (Directory.Exists (folder)) {
				string[] userFolders = Directory.GetDirectories(folder);
				for (int i = 0; i < userFolders.Length; i++){
                    string[] jsonFile = Directory.GetFiles(userFolders[i], "*.json");
                    if (jsonFile.Length == 1) {
                        SaveVersions.Version_Current.User userSave = FileManager.LoadObject<SaveVersions.Version_Current.User>(jsonFile[0], false);
                        AddUser(new User(userSave));
                    }
                    else {
                        Logger.LogError("Only one json file allowed in user save folder: ");
                        Logger.LogError(userFolders[i]);
                    }
				}
			} else
				Directory.CreateDirectory (folder);
		}

		public void Save(string folder){
			if (!Directory.Exists (folder))
				Directory.CreateDirectory (folder);

			User[] userList = GetUsers ();
			for (int i = 0; i < userList.Length; i++) {
				SaveVersions.Version_Current.User userSave = userList[i].GetSerializer();
                string userFoler = folder + userSave.name + GameServer.sepChar;
                if (!Directory.Exists(userFoler))
                    Directory.CreateDirectory(userFoler);
                FileManager.SaveConfigFile(userFoler + userSave.name + ".json", userSave, false);
			}

			//Logger.Log ("{0} users saved.", userList.Length.ToString());
		}
	}
}

