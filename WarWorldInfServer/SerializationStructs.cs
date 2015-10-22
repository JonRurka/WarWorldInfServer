using System;
using System.Collections.Generic;
using System.Drawing;
using LibNoise;

// TODO: put this in own project.
public class SerializationStructs
{
	public enum ResponseType{
		Failed,
		Successfull,
	}

	// temp color transfer.
	public struct TColor
	{

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
		public WarWorldInfServer.User.PermissionLevel permission;
		public string sessionKey;
		public string message;
		
		public LoginResponse(ResponseType response, WarWorldInfServer.User.PermissionLevel permission, string sessionKey, string message){
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
		public List<GradientPresets.GradientKeyData> gradient;
		public string[] terrainImages;
		public string message;
		
		public MapData(ResponseType response, int seed, int width, int height, List<GradientPresets.GradientKeyData> gradient, string[] terrainImages, string message){
			this.response = response;
			this.seed = seed;
			this.width = width;
			this.height = height;
			this.gradient = gradient;
			this.terrainImages = terrainImages;
			this.message = message;
		}
	}

	public struct ImageFileData{
		public string file;
		public List<Color> image;
		
		public ImageFileData(string file, List<Color> image){
			this.file = file;
			this.image = image;
		}
	}
}


