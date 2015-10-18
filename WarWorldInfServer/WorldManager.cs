using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace WarWorldInfServer
{

	public class WorldManager
	{
		private GameServer _server;
		private Dictionary<string, string> _worldFolders;

		public World CurrentWorld { get; private set; }
		public string MainWorldDirectory { get; private set; }

		public WorldManager (GameServer server)
		{
			_server = server;
			_worldFolders = new Dictionary<string, string> ();
			MainWorldDirectory = Directory.GetCurrentDirectory() + "/Worlds/";
			if (!Directory.Exists (MainWorldDirectory)) {
				Directory.CreateDirectory (MainWorldDirectory);
				Logger.Log("World Directory created.");
			}

			string[] worldDirectories = Directory.GetDirectories (MainWorldDirectory);
			for (int i = 0; i < worldDirectories.Length; i++) {
				string folderName = Path.GetFileName(Path.GetDirectoryName(worldDirectories[i] + "/"));
				_worldFolders[folderName] = worldDirectories[i] + "/";
				//Logger.Log(folderName);
			}

			//Logger.Log ("World Manager initialized.");
		}

		public World LoadWorld(string worldName){
			CurrentWorld = new World (this).LoadWorld(worldName);
			return CurrentWorld;
		}

		public void SaveCurrentWorld(){
			SaveWorld (CurrentWorld.WorldName);
		}

		public void SaveWorld(string worldName){
			if (WorldExists (worldName)) {
				CurrentWorld.Save (worldName);
			} else {
				CurrentWorld = CreateWorld(worldName);
			}
		}

		public World CreateWorld(string worldName){
			if (!WorldExists (worldName)) {
				CurrentWorld = new World (this).CreateNewWorld (worldName);
				return CurrentWorld;
			}
			Logger.LogWarning ("Loading world \"{0}\" as it already exists.", worldName);
			CurrentWorld = new World (this).LoadWorld (worldName);
			return CurrentWorld;
		}

		public void AddWorldDirectory(string worldName, string directory){
			if (!WorldExists (worldName))
				_worldFolders [worldName] = directory;
		}

		public string GetWorldDirectory(string worldName){
			if (WorldExists (worldName))
				return _worldFolders [worldName];
			return string.Empty;
		}

		public bool WorldExists(string worldName){
			return _worldFolders.ContainsKey (worldName.ToLower ()) && Directory.Exists(_worldFolders[worldName]);
		}
	}
}

