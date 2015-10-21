using System;
using System.IO;
using System.Collections.Generic;
using LibNoise;

namespace WarWorldInfServer
{
	public class World
	{
		public struct WorldConfigSave
		{
			public string version;
			public Time time;
			public TerrainBuilder.TerrainSettings terrain;
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

		//private string _worldName;
		//private string _worldDirectory;
		private WorldManager _worldManager;
		//private Time _time;
		//private TerrainBuilder _terrain;

		public string WorldName { get; private set; }
		public string WorldDirectory { get; private set; }
		public Time WorldStartTime{ get; private set; }
		public TerrainBuilder Terrain { get; private set; }

		public World (WorldManager worldManager)
		{
			_worldManager = worldManager;
		}

		public World CreateNewWorld(string worldName){
			WorldDirectory = _worldManager.MainWorldDirectory + worldName + "/";
			if (!Directory.Exists (WorldDirectory))
				Directory.CreateDirectory (WorldDirectory);
			_worldManager.AddWorldDirectory (worldName, WorldDirectory);

			//TODO: create config and save files.
			SettingsLoader settings = GameServer.Instance.Settings;

			IModule module = new Perlin ();
			((Perlin)module).OctaveCount = 16;
			((Perlin)module).Seed = settings.TerrainSeed;

			Terrain = new TerrainBuilder (settings.TerrainWidth, settings.TerrainHeight, settings.TerrainSeed);
			Terrain.Generate (module, settings.TerrainPreset);
			Terrain.Save(WorldDirectory + settings.TerrainImageFile);
			Terrain.SaveModule(WorldDirectory + settings.TerrainModuleFile);

			WorldName = worldName;
			WorldConfigSave worldSave = new WorldConfigSave ();
			WorldStartTime = new Time (0, 0, 0, 0, 0, 0, settings.SecondsInTicks);
			worldSave.version = GameServer.Instance.Version;
			worldSave.time = WorldStartTime;
			worldSave.terrain = Terrain.Settings;
			FileManager.SaveConfigFile(WorldDirectory + settings.WorldSaveFile, worldSave, false);

			GameServer.Instance.Users.Save(WorldDirectory + "Users/");

			Logger.Log ("World \"{0}\" created.", worldName);
			GameServer.Instance.StartWorld (this);

			return this;
		}

		public World LoadWorld(string worldName){
			if (_worldManager.WorldExists (worldName)) {
				WorldDirectory = _worldManager.GetWorldDirectory(worldName);

				//TODO: Load world config and save files.
				SettingsLoader settings = GameServer.Instance.Settings;

				WorldName = worldName;
				WorldConfigSave worldSave = (WorldConfigSave)FileManager.LoadObject<WorldConfigSave>(WorldDirectory + settings.WorldSaveFile, false);
				WorldStartTime = worldSave.time;
				Terrain = new TerrainBuilder(worldSave.terrain);
				GameServer.Instance.Users.LoadUsers(WorldDirectory + "Users/");

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
			WorldDirectory = _worldManager.MainWorldDirectory + worldName + "/";
			SettingsLoader settings = GameServer.Instance.Settings;

			WorldConfigSave worldSave = new WorldConfigSave ();
			GameTimer timer = GameServer.Instance.GameTime;
			worldSave.version = GameServer.Instance.Version;
			worldSave.time = new Time (timer.TotalSeconds, timer.Minutes, timer.Hours, timer.Days, timer.Tick, timer.SecondsInTick, timer.MaxSecondsInTick );
			worldSave.terrain = Terrain.Settings;
			FileManager.SaveConfigFile (WorldDirectory + settings.WorldSaveFile, worldSave, false);

			if (!File.Exists(WorldDirectory + settings.TerrainImageFile))
				Terrain.Save (WorldDirectory + settings.TerrainImageFile);
			GameServer.Instance.Users.Save (WorldDirectory + "Users/");
			
			Logger.Log ("World \"{0}\" saved.", worldName);
		}
	}
}

