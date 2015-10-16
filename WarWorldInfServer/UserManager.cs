using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace WarWorldInfServer
{
	public class UserManager
	{
		private Dictionary<string, User> _users;
		/* Holds the lists of users.
		 * 
		 */ 


		public UserManager ()
		{
			_users = new Dictionary<string, User> ();
		}

		public User LoadCreateUser(string name){
			if (UserExists (name))
				return GetUser (name);
			return CreateUser (name);
		}

		public User CreateUser(string name){
			User user = new User ();
			user.Name = name;
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

		public void AddUser(User user){
			if (!UserExists (user.Name)) {
				_users.Add(user.Name, user);
			}
		}

		public void LoadUsers(string folder){
			if (Directory.Exists (folder)) {
				string[] userFiles = Directory.GetFiles(folder, "*.json");
				for (int i = 0; i < userFiles.Length; i++){
					SaveVersions.Version_Current.User userSave = FileManager.LoadObject<SaveVersions.Version_Current.User>(userFiles[i]);
					AddUser(new User(userSave));
				}
			} else
				Directory.CreateDirectory (folder);
		}

		public void Save(string folder){
			if (!Directory.Exists (folder))
				Directory.CreateDirectory (folder);

			User[] userList = GetUsers ();
			foreach (User user in userList) {
				SaveVersions.Version_Current.User userSave = user.GetSerializer();
				FileManager.SaveConfigFile(folder + userSave.name + ".json", userSave);
			}

			Logger.Log ("{0} users saved.", userList.Length);
		}
	}
}

