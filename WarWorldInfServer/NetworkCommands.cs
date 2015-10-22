using System;
using System.Net;
using System.Collections.Generic;
using LibNoise;
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
			_server.Net.AddCommand ("traffic", Traffic_CMD);
			_server.Net.AddCommand ("getterrain", GetTerrain_CMD);
		}

		private void Ping_CMD(IPEndPoint endPoint, string args){
			//Logger.Log ("Received ping from " + endPoint.Address.ToString ());
			_server.Net.Send (endPoint, "pingsuccess" + NetServer.Delimiter);
		}

		private void GetSalt_CMD(IPEndPoint endPoint, string args){
			string salt = _server.DB.GetSalt (args);
			_server.Net.Send (endPoint, "completelogin" + NetServer.Delimiter + salt);
		}
		
		private void Login_CMD(IPEndPoint endPoint, string args){
			try {
				SerializationStructs.Login loginData = JsonConvert.DeserializeObject<SerializationStructs.Login> (args);
				SerializationStructs.ResponseType responseType = SerializationStructs.ResponseType.Failed;
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
						responseType = loggedIn ? SerializationStructs.ResponseType.Successfull : SerializationStructs.ResponseType.Failed;
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
				_server.Net.Send(endPoint, "loginresponse" + NetServer.Delimiter + JsonConvert.SerializeObject(response));
			}
			catch (Exception e){
				Logger.LogError(e.StackTrace);
			}
		}

		private void Traffic_CMD(IPEndPoint endPoint, string args){
			//Logger.Log ("traffic");
			_server.Net.Send (endPoint, "traffic" + NetServer.Delimiter + args);
		}

		private void GetTerrain_CMD(IPEndPoint endPoint, string args){
			SerializationStructs.ResponseType responseType = SerializationStructs.ResponseType.Failed;
			int seed = 0;
			int width = 0;
			int height = 0;
			LibNoise.IModule module = null;
			List<GradientPresets.GradientKeyData> gradient = new List<GradientPresets.GradientKeyData>();
			string[] TextureFiles = new string[0];
			string message = string.Empty;

			if (_server.WorldLoaded) {
				if (_server.Users.SessionKeyExists (args)) {
					TerrainBuilder builder = _server.Worlds.CurrentWorld.Terrain;
					seed = builder.Seed;
					width = builder.Width;
					height = builder.Height;
					module = builder.NoiseModule;
					gradient = new List<GradientPresets.GradientKeyData>(builder.GradientPreset);
					TextureFiles = new List<string> (GradientPresets.TextureFiles.Keys).ToArray ();
					message = "success";
				} else
					message = "Invalid session key";
			} else {
				message = "World not loaded";
			}

			// make sure images are cleared. They will be sent seperatly.
			for (int i = 0; i < gradient.Count; i++) {
				gradient[i].images.Clear(); 
			}

			SerializationStructs.MapData data = new SerializationStructs.MapData (responseType, seed, width, height, gradient, TextureFiles, message);
			string sendStr = JsonConvert.SerializeObject (
				data, Formatting.Indented);
			//Logger.Print (sendStr.Length * sizeof(char));
			 //Logger.PrintNoFormat(sendStr);
			string moduleStr = JsonConvert.SerializeObject (module, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.All});
			_server.Net.Send (endPoint, "setterrainmodule" + NetServer.Delimiter + moduleStr);

			if (sendStr.Length * sizeof(char) <= 65536)
				_server.Net.Send (endPoint, "setterraindata" + NetServer.Delimiter + sendStr);
			else {
				Logger.Log ("map data is to big to send.");
				Logger.PrintNoFormat(sendStr);
			}

			foreach (string imageName in GradientPresets.TextureFiles.Keys) {
				List<System.Drawing.Color> image = new List<System.Drawing.Color>(GradientPresets.TextureFiles[imageName]);
				Libnoise.SerializationStructs.ImageFileData imageStruct = new Libnoise.SerializationStructs.ImageFileData(imageName, image);
				string imageStr = JsonConvert.SerializeObject(imageStruct, Formatting.Indented, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.All});
				if (imageStr.Length * sizeof(char) <= 65536)
					_server.Net.Send(endPoint, "setimage" + NetServer.Delimiter + imageStr);
				else
					Logger.Log("{0} is to big to send.", imageName);
			}
		}
	}
}

