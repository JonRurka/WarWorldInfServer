using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace WarWorldInfServer
{

	public class WorldManager
	{
		private string _mainWorldDirectory;
		private World _currentWorld;
		private GameServer _server;
		private Dictionary<string, string> _worldFolders;

		public World CurrentWorld { get { return _currentWorld; } }
		public string MainWorldDirectory { get { return _mainWorldDirectory; } }

		public WorldManager (GameServer server)
		{
			_server = server;
			_worldFolders = new Dictionary<string, string> ();
			_mainWorldDirectory = Directory.GetCurrentDirectory() + "/Worlds/";
			if (!Directory.Exists (_mainWorldDirectory)) {
				Directory.CreateDirectory (_mainWorldDirectory);
				Logger.Log("World Directory created.");
			}

			string[] worldDirectories = Directory.GetDirectories (_mainWorldDirectory);
			for (int i = 0; i < worldDirectories.Length; i++) {
				string folderName = Path.GetFileName(Path.GetDirectoryName(worldDirectories[i] + "/"));
				_worldFolders[folderName] = worldDirectories[i] + "/";
				//Logger.Log(folderName);
			}

			Logger.Log ("World Manager initialized.");
		}

		public World LoadWorld(string worldName){
			return new World (this).LoadWorld(worldName);
		}

		public void SaveCurrentWorld(){
			SaveWorld (_currentWorld.WorldName);
		}

		public void SaveWorld(string worldName){
			if (WorldExists (worldName)) {
				_currentWorld.Save (worldName);
				Logger.LogWarning ("World \"{0}\" saved.", worldName);
			} else {
				_currentWorld = CreateWorld(worldName);
			}
		}

		public World CreateWorld(string worldName){
			if (!WorldExists (worldName)) {
				return new World (this).CreateNewWorld (worldName);
				Logger.Log ("World \"{0}\" created.", worldName);
			}
			Logger.LogWarning ("Loading world \"{0}\" as it already exists.", worldName);
			return new World (this).LoadWorld (worldName);
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

