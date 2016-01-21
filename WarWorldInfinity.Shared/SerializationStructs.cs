using System;
using System.Collections.Generic;
using WarWorldInfinity.LibNoise;
using WarWorldInfinity.Shared.Structures;
using SimpleJSON;

namespace WarWorldInfinity.Shared {
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
        public string standings;
        public IExtraData extraData;

        public Structure(Vector2Int position, string type, string owner, string alliance, string standings, IExtraData extraData) {
            this.position = position;
            this.type = type;
            this.owner = owner;
            this.alliance = alliance;
            this.standings = standings;
            this.extraData = extraData;
        }

        /*public Structure(string jsonStr) {
            // json convert
        }

        public override string ToString() {
            // json convert.
            return "";
        }*/
    }

    public struct SquadInfo {
        public enum SquadStanding {
            None,
            Own,
            Ally,
            Nuetral,
            Enemy,
        }
        public string owner;
        public string name;
        public string ident;
        public SquadStanding standings;
        public Vector2Int source;
        public Vector2Int destination;
        public Vector2Int location;
        public bool isTraveling;

        // Unit Details.

        public SquadInfo(string owner, string name, string ident, SquadStanding standings, Vector2Int source, Vector2Int destination, Vector2Int location, bool isTraveling) {
            this.owner = owner;
            this.name = name;
            this.ident = ident;
            this.standings = standings;
            this.source = source;
            this.destination = destination;
            this.location = location;
            this.isTraveling = isTraveling;
        }
    }

    public struct Traffic {
        public string command;
        public string data;

        public Traffic(string command, string data) {
            this.command = command;
            this.data = data;
        }

        /*public Traffic(string jsonStr) {

        }

        public override string ToString() {
            // json convert.
            return "";
        }*/
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

        /*public Message(string jsonStr) {
            
        }

        public override string ToString() {
            // json convert.
            return "";
        }*/
    }

    public struct ChatMessage {
        public string sessionKey;
        public string player;
        public string message;
        
        public ChatMessage(string sessionKey, string player, string message) {
            this.sessionKey = sessionKey;
            this.player = player;
            this.message = message;
        }
    }

    public enum MessageTypes {
        None,
        Success,
        Not_Logged_in,
        User_Not_Found,
        World_Not_Loaded,
        Invalid_Structure_Location,
        Invalid_Op_Age,
        No_OP,
        Op_Not_Upgradable,
        Not_Enough_Resources,
        Invalid_Permission
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

        public override string ToString() {
            // json convert.
            return "";
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

        public override string ToString() {
            // json convert.
            return "";
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

        public override string ToString() {
            // json convert.
            return "";
        }
    }

    public struct StructureCommand {
        public string sessionKey;
        public Vector2Int location;
        public string command;
        public string args;

        public StructureCommand(string sessionKey, Vector2Int location, string command, string args) {
            this.sessionKey = sessionKey;
            this.location = location;
            this.command = command;
            this.args = args;
        }

        public override string ToString() {
            // json convert.
            return "";
        }
    }

    // Structs sent to client from server.

    public struct LoginResponse {
        public ResponseType response;
        public string permission;
        public string sessionKey;
        public int tick;
        public string message;

        public LoginResponse(ResponseType response, string permission, string sessionKey, int tick, string message) {
            this.response = response;
            this.permission = permission;
            this.sessionKey = sessionKey;
            this.tick = tick;
            this.message = message;
        }

        /*public LoginResponse(string jsonStr) {
            
        }*/
    }

    public struct StructureSquads {
        public Vector2Int location;
        public SquadInfo[] squads;

        public StructureSquads(Vector2Int location, SquadInfo[] squads) {
            this.location = location;
            this.squads = squads;
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

        /*public MapData(string jsonStr) {
            
        }*/
    }

    public struct ImageFileData{
		public string file;
		public Color[] image;
			
		public ImageFileData(string file, Color[] image){
			this.file = file;
			this.image = image;
		}

        /*public ImageFileData(string jsonStr) {
            
        }*/
	}

    public struct CommandList {
        public string type;
        public string[] commands;

        public CommandList(string type, string[] commands) {
            this.type = type;
            this.commands = commands;
        }

        /*public CommandList(string jsonStr) {
            
        }*/
    }
}


