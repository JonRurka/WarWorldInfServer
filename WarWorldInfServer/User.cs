using System;
using System.Diagnostics;

namespace WarWorldInfServer
{
	public class User
	{
		public enum PermissionLevel
		{
			None,
			Observer,
			User,
			Moderator,
			Admin
		}

		public string Name { get; set; }
		public string PasswordHash { get; set;}
		public string Ip { get; set; }
		public string SessionKey { get; set; }
		public bool Connected { get; set;}
		public Stopwatch TimeSinceInteraction { get; private set;}
		public PermissionLevel Permission { get; private set;}
		public string LoginMessage {get; private set;}

		// Store name, passwordhash, and permission level in sql database.
		// Use Xampp

		/* Contains all information for a user:
		 * network info (Ip, port, session key).
		 * resources
		 * outposts
		 * units (or stored in outposts).
		 * last known camera location.
		 */

		public User(){
			TimeSinceInteraction = new Stopwatch ();
		}

		public User (SaveVersions.Version_Current.User user) : this(){
			Deserialize (user);
		}

		public bool Login(string ip, string passwordHash){
			if (passwordHash == PasswordHash) {
				Ip = ip;
				SessionKey = SessionKeyGenerator.RandomKey (GameServer.Instance.Settings.SessionKeyLength);
				LoginMessage = "success";
				Connected = true;
				ResetTimer ();
				return true;
			}
			SessionKey = "";
			LoginMessage = "Invalid password.";
			return false;
		}

		public SaveVersions.Version_Current.User GetSerializer(){
			SaveVersions.Version_Current.User user = new SaveVersions.Version_Current.User ();
			user.name = Name;
			user.permission = Permission;
			user.passwordHash = PasswordHash;
			return user;
		}

		public void Deserialize(SaveVersions.Version_Current.User user){
			Name = user.name;
			Permission = user.permission;
			PasswordHash = user.passwordHash;
		}

		public void ResetTimer(){
			TimeSinceInteraction.Reset ();
			TimeSinceInteraction.Start ();
		}

		public bool CheckIfTimeout(){
			return TimeSinceInteraction.Elapsed.Seconds >= GameServer.Instance.Settings.UserTimeoutTime;
		}
	}
}
