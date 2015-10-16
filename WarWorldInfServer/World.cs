using System;
using System.IO;
namespace WarWorldInfServer
{
	public class World
	{
		public struct WorldConfigSave
		{
			public string version;
			public Time time;
			public TerrainSettings terrain;
		}

		public struct Time
		{
			public int seconds;
			public int minutes;
			public int hours;
			public int days;
			public int tick;
			public int secondsInTicks;
			public int maxSecondsInTicks;

			public Time(int seconds, int minutes, int hours, int days, int tick, int secondsInTicks, int maxSecondsInTicks){
				this.seconds = seconds;
				this.minutes = minutes;
				this.hours = hours;
				this.days = days;
				this.tick = tick;
				this.secondsInTicks = secondsInTicks;
				this.maxSecondsInTicks = maxSecondsInTicks;
			}

			public Time(SaveVersions.Version_Current.Time time){
				this.seconds = time.seconds;
				this.minutes = time.minutes;
				this.hours = time.hours;
				this.days = time.days;
				this.tick = time.tick;
				this.secondsInTicks = time.secondsInTicks;
				this.maxSecondsInTicks = time.maxSecondsInTicks;
			}
		}
		public struct TerrainSettings
		{
			public int width;
			public int height;
			public int seed;
			public string imageFile;

			public TerrainSettings(int width, int height, int seed, string imageFile){
				this.width = width;
				this.height = height;
				this.seed = seed;
				this.imageFile = imageFile;
			}
		}


		private string _worldName;
		private string _worldDirectory;
		private WorldManager _worldManager;
		private Time _time;
		private TerrainBuilder _terrain;

		public string WorldName { get { return _worldName; } }
		public string WorldDirectory { get { return _worldDirectory; } }
		public Time WorldStartTime{ get { return _time; } }
		public TerrainBuilder Terrain { get { return _terrain; } }

		public World (WorldManager worldManager)
		{
			_worldManager = worldManager;
		}

		public World CreateNewWorld(string worldName){
			_worldDirectory = _worldManager.MainWorldDirectory + worldName + "/";
			if (!Directory.Exists (_worldDirectory))
				Directory.CreateDirectory (_worldDirectory);
			_worldManager.AddWorldDirectory (worldName, _worldDirectory);

			//TODO: create config and save files.
			_worldName = worldName;
			SettingsLoader settings = GameServer.Instance.Settings;
			WorldConfigSave worldSave = new WorldConfigSave ();
			_time = new Time (0, 0, 0, 0, 0, 0, settings.SecondsInTicks);
			worldSave.version = GameServer.Instance.Version;
			worldSave.time = _time;
			worldSave.terrain = new TerrainSettings (settings.TerrainWidth, settings.TerrainHeight, settings.TerrainSeed, settings.TerrainImageFile);
			FileManager.SaveConfigFile(_worldDirectory + "WorldSave.json", worldSave);

			_terrain = new TerrainBuilder (worldSave.terrain.width, worldSave.terrain.height, worldSave.terrain.height);
			_terrain.Generate (LibNoise.GradientPresets.Terrain);
			_terrain.Save(_worldDirectory + "Map.bmp");

			GameServer.Instance.Users.Save (_worldDirectory + "Users/");

			Logger.Log ("World \"{0}\" created.", worldName);
			GameServer.Instance.StartWorld (this);

			return this;
		}

		public World LoadWorld(string worldName){
			if (_worldManager.WorldExists (worldName)) {
				_worldDirectory = _worldManager.GetWorldDirectory(worldName);

				//TODO: Load world config and save files.
				_worldName = worldName;
				WorldConfigSave worldSave = (WorldConfigSave)FileManager.LoadObject<WorldConfigSave>(_worldDirectory + "WorldSave.json");
				_time = worldSave.time;
				_terrain = new TerrainBuilder(_worldDirectory + "Map.bmp");
				GameServer.Instance.Users.LoadUsers(_worldDirectory + "Users/");

				Logger.Log("World \"{0}\" loaded.", worldName);
				GameServer.Instance.StartWorld(this);

			} else {
				Logger.LogWarning ("Created world \"{0}\" as it does not exists.", worldName);
				CreateNewWorld(worldName);
			}

			return this;
		}

		public void Save(string worldName){
			// Save changes to files
			_worldDirectory = _worldManager.MainWorldDirectory + worldName + "/";

			WorldConfigSave worldSave = new WorldConfigSave ();
			GameTimer timer = GameServer.Instance.GameTime;
			worldSave.version = GameServer.Instance.Version;
			worldSave.time = new Time (timer.Seconds, timer.Minutes, timer.Hours, timer.Days, timer.Tick, timer.SecondsInTick, timer.MaxSecondsInTick );
			FileManager.SaveConfigFile (_worldDirectory + "WorldSave.json", worldSave);

			_terrain.Save (_worldDirectory + "Map.bmp");
			GameServer.Instance.Users.Save (_worldDirectory + "Users/");
			
			Logger.Log ("World \"{0}\" saved.", worldName);
		}
	}
}

