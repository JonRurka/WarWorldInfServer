using System;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace WarWorldInfServer
{
	public class DataBase
	{
		public string Database { get; private set;}
		public string Server { get; private set;}

		private MySqlConnection _connection;

		public DataBase(string server, string dataBase){
			Database = dataBase;
			Server = server;
		}

		public void Connect(string user, string password){
			string conStr = string.Format ("Server={0}; database={1}; UID={2}; password={3}", Server, Database, user, password);
			_connection = new MySqlConnection (conStr);
			_connection.Open ();

			ExecuteQuery ("CREATE TABLE IF NOT EXISTS `users`(" +
			              "userid int(6) NOT NULL AUTO_INCREMENT," +
			              "username varchar(255) NOT NULL," +
			              "password varchar(255) NOT NULL," +
			              "salt varchar(255) NOT NULL," +
			              "permission varchar(255) NOT NULL," +
			              "email varchar(255) NOT NULL," +
			              "PRIMARY KEY (userid)" +
			              ") ENGINE=InnoDB DEFAULT CHARSET=utf8 " +
			              "COLLATE=utf8_unicode_ci AUTO_INCREMENT=1;").Close();
		}

		public MySqlDataReader ExecuteQuery(string query, params string[] args){
			MySqlCommand cmd = new MySqlCommand (string.Format(query, args), _connection);
			for (int i = 0; i < args.Length; i++) {

			}
			return cmd.ExecuteReader ();
		}

		public void AddNewPlayer(string user, string password, string salt, string permission, string email){
			string query = "INSERT INTO users (username, password, salt, permission, email) VALUES (@user, @password, @salt, @permission, @email);";
			MySqlCommand cmd = new MySqlCommand (query, _connection);
			cmd.Parameters.AddWithValue ("@user" ,user);
			cmd.Parameters.AddWithValue ("@password", password);
			cmd.Parameters.AddWithValue ("@salt", salt);
			cmd.Parameters.AddWithValue ("@permission", permission);
			cmd.Parameters.AddWithValue ("@email", email);
			cmd.ExecuteReader ().Close();
			cmd.Dispose ();
		}

		public int GetUserID(string user){
			string query = "SELECT userid from users WHERE username=@username";
			MySqlCommand cmd = new MySqlCommand (query, _connection);
			cmd.Parameters.AddWithValue ("@username", user);
			int id = (int)(cmd.ExecuteScalar () ?? 0);
			cmd.Dispose ();
			return id;
		}

		public string GetUserName(int id){
			string query = "SELECT username from users WHERE userid=@userid";
			MySqlCommand cmd = new MySqlCommand (query, _connection);
			cmd.Parameters.AddWithValue ("@userid", id);
			string username = (cmd.ExecuteScalar () ?? String.Empty).ToString();
			cmd.Dispose ();
			return username;
		}

		public string GetPassword(string user){
			string query = "SELECT password FROM users WHERE username=@username";
			MySqlCommand cmd = new MySqlCommand (query, _connection);
			cmd.Parameters.AddWithValue ("@username", user);
			string passwordStr = (cmd.ExecuteScalar () ?? String.Empty).ToString();
			cmd.Dispose ();
			return passwordStr;
		}

		public string GetSalt(string user){
			string query = "SELECT salt FROM users WHERE username=@username";
			MySqlCommand cmd = new MySqlCommand (query, _connection);
			cmd.Parameters.AddWithValue ("@username", user);
			string saltStr = (cmd.ExecuteScalar () ?? String.Empty).ToString();
			cmd.Dispose ();
			return saltStr;
		}

		public string GetPermission(string user){
			string query = "SELECT permission FROM users WHERE username=@username";
			MySqlCommand cmd = new MySqlCommand (query, _connection);
			cmd.Parameters.AddWithValue ("@username", user);
			string permissionStr = (cmd.ExecuteScalar () ?? String.Empty).ToString();
			cmd.Dispose ();
			return permissionStr;
		}

		public string GetEmail(string user){
			string query = "SELECT email FROM users WHERE username=@username";
			MySqlCommand cmd = new MySqlCommand (query, _connection);
			cmd.Parameters.AddWithValue ("@username", user);
			string emailStr = (cmd.ExecuteScalar () ?? String.Empty).ToString();
			cmd.Dispose ();
			return emailStr;
		}

		public bool UserExists(string user){
			string query = "SELECT COUNT(*) FROM users WHERE username=@username";
			MySqlCommand cmd = new MySqlCommand (query, _connection);
			cmd.Parameters.AddWithValue ("@username", user);
			int users = 0;
			int.TryParse ((cmd.ExecuteScalar () ?? "0").ToString(), out users);
			cmd.Dispose ();
			return users > 0;
		}
	}
}

