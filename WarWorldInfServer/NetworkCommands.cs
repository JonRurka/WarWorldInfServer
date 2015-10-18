using System;
using System.Net;
using Newtonsoft.Json;

namespace WarWorldInfServer
{
	public class NetworkCommands
	{
		GameServer _server;

		public NetworkCommands ()
		{
			_server = GameServer.Instance;
			_server.Net.AddCommand ("ping", Ping_CMD);
			_server.Net.AddCommand ("login", Login_CMD);
			_server.Net.AddCommand ("getsalt", GetSalt_CMD);
		}

		private void Ping_CMD(IPEndPoint endPoint, string args){
			//Logger.Log ("Received ping from " + endPoint.Address.ToString ());
			_server.Net.Send (endPoint.Address.ToString (), _server.Net.ClientPort, "pingsuccess#");
		}

		private void GetSalt_CMD(IPEndPoint endPoint, string args){
			string salt = _server.DB.GetSalt (args);
			_server.Net.Send (endPoint.Address.ToString (), _server.Net.ClientPort, "completelogin#" + salt);
		}
		
		private void Login_CMD(IPEndPoint endPoint, string args){
			try {
				SerializationStructs.Login loginData = JsonConvert.DeserializeObject<SerializationStructs.Login> (args);
				SerializationStructs.LoginResponse.ResponseType responseType = SerializationStructs.LoginResponse.ResponseType.Failed;
				User.PermissionLevel permission = User.PermissionLevel.None;
				string sessionKey = string.Empty;
				string message = string.Empty;
				if (_server.WorldLoaded) {
					if (_server.DB.UserExists(loginData.name)){
						User user;
						if (_server.Users.UserExists(loginData.name)){
							user = _server.Users.GetUser (loginData.name);
						}
						else{
							user = _server.Users.CreateUser(loginData.name);
						}
						bool loggedIn = user.Login (endPoint.Address.ToString (), HashHelper.HashPasswordServer(loginData.password, loginData.salt));
						responseType = loggedIn ? SerializationStructs.LoginResponse.ResponseType.Successfull : SerializationStructs.LoginResponse.ResponseType.Failed;
						permission = loggedIn ? user.Permission : User.PermissionLevel.None;
						sessionKey = user.SessionKey;
						message = user.LoginMessage;
						Logger.Log("User {0} logged in from {1}", loginData.name, endPoint.Address.ToString());
					}
					else{
						message = "User not found!";
					}
				} else {
					message = "No world instance loaded!";
					//Logger.LogWarning("User {0} tried to log in with no world loaded!", loginData.name);
				}
				
				SerializationStructs.LoginResponse response = new SerializationStructs.LoginResponse (responseType, permission, sessionKey, message);
				_server.Net.Send(endPoint.Address.ToString(), _server.Net.ClientPort, "loginresponse#" + JsonConvert.SerializeObject(response));
			}
			catch (Exception e){
				Logger.LogError(e.StackTrace);
			}
		}
	}
}

