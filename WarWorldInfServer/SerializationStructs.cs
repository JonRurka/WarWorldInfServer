using System;
using System.Collections.Generic;
using LibNoise;

namespace WarWorldInfServer
{
	public class SerializationStructs
	{
		public enum ResponseType{
			Failed,
			Successfull,
		}

		// Structs sent to server from client

		// Sent from client when logging on.
		public struct Login{
			public string name;
			public string password;
			public string salt;
			
			public Login(string name, string password, string salt){
				this.name = name;
				this.password = password;
				this.salt = salt;
			}
		}

		// Structs sent to client from server.
		public struct LoginResponse{
			public ResponseType response;
			public User.PermissionLevel permission;
			public string sessionKey;
			public string message;

			public LoginResponse(ResponseType response, User.PermissionLevel permission, string sessionKey, string message){
				this.response = response;
				this.permission = permission;
				this.sessionKey = sessionKey;
				this.message = message;
			}
		}

		// Basic map data sent to client after login.
		public struct MapData{
			public ResponseType response;
			public int seed;
			public int width;
			public int height;
			public IModule module;
			public List<GradientPresets.GradientKeyData> preset;
			public string message;

			public MapData(ResponseType response, int seed, int width, int height, IModule module, List<GradientPresets.GradientKeyData> preset, string message){
				this.response = response;
				this.seed = seed;
				this.width = width;
				this.height = height;
				this.module = module;
				this.preset = preset;
				this.message = message;
			}
		}
	}
}

