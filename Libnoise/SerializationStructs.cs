using System;
using System.Collections.Generic;

namespace LibNoise.SerializationStructs {
	/// <summary>
	/// All SerializationStructs that require type information
	/// when serializing.
	/// </summary>


    public enum ResponseType {
        Failed,
        Successfull,
    }

    public struct Structure {
        public Vector2Int position;
        public string type;
        public string owner;
        public string alliance;

        public Structure(Vector2Int position, string type, string owner, string alliance) {
            this.position = position;
            this.type = type;
            this.owner = owner;
            this.alliance = alliance;
        }
    }

    public struct Traffic {
        public string command;
        public string data;

        public Traffic(string command, string data) {
            this.command = command;
            this.data = data;
        }
    }

    public struct Message {
        public string ident;
        public MessageTypes type;
        public string message;

        public Message(string ident, MessageTypes type, string message) {
            this.ident = ident;
            this.type = type;
            this.message = message;
        }
    }

    public enum MessageTypes {
        None,
        Success,
        Not_Logged_in,
        World_Not_Loaded,
        Invalid_Structure_Location,
        Invalid_Op_Age,
        No_OP,
        Op_Not_Upgradable,
        Not_Enough_Resources,
    }

    // Structs sent to server from client

    // Sent from client when logging on.
    public struct Login {
        public string name;
        public string password;
        public string salt;

        public Login(string name, string password, string salt) {
            this.name = name;
            this.password = password;
            this.salt = salt;
        }
    }

    public struct GetStructures {
        public enum RequestType {
            All,
            Owned,
            Visible,
        }
        public string sessionKey;
        public RequestType requestType;
        public bool onlyChanged;
        public Vector2Int topLeft;
        public Vector2Int bottomRight;

        public GetStructures(string sessionKey, RequestType requestType, bool onlyChanged, Vector2Int topLeft, Vector2Int bottomRight) {
            this.sessionKey = sessionKey;
            this.requestType = requestType;
            this.onlyChanged = onlyChanged;
            this.topLeft = topLeft;
            this.bottomRight = bottomRight;
        }
    }

    public struct SetStructure {
        public string sessionKey;
        public Vector2Int location;
        public string type;

        public SetStructure(string sessionKey, Vector2Int location, string type) {
            this.sessionKey = sessionKey;
            this.location = location;
            this.type = type;
        }
    }


    // Structs sent to client from server.


    public struct LoginResponse {
        public ResponseType response;
        public string permission;
        public string sessionKey;
        public string message;

        public LoginResponse(ResponseType response, string permission, string sessionKey, string message) {
            this.response = response;
            this.permission = permission;
            this.sessionKey = sessionKey;
            this.message = message;
        }
    }

    // Basic map data sent to client after login.
    public struct MapData {
        public ResponseType response;
        public int seed;
        public int width;
        public int height;
        public List<GradientPresets.GradientKeyData> gradient;
        public string[] terrainImages;
        public string message;

        public MapData(ResponseType response, int seed, int width, int height, List<GradientPresets.GradientKeyData> gradient, string[] terrainImages, string message) {
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
		public Color[] image;
			
		public ImageFileData(string file, Color[] image){
			this.file = file;
			this.image = image;
		}
	}
}


