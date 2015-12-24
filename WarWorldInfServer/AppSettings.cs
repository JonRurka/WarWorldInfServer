using System;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using IniParser;
using IniParser.Parser;
using IniParser.Model;


namespace WarWorldInfinity
{
	public class AppSettings
	{
		public static class Defaults{
			public static int SessionKeyLength { get { return 32; } }
			public static int ServerPort { get { return 9999; } }
			public static int ClientPort { get { return 9999; } }
			public static int SecondsInTicks { get { return 3600; } }
			public static int UserTimeoutTime { get { return 3600; } }
			public static string WorldSaveFile { get { return "WorldSave.json"; } }
			public static int AutoSaveInterval { get { return 300; } }
            public static int MinOpDistance { get { return 5; } }
            public static int BaseRadarRadius { get { return 1000; } }

            public static int TerrainWidth { get { return 1920; } }
			public static int TerrainHeight { get { return 1080; } }
			public static int TerrainSeed { get { return 0; } }
			public static string TerrainPreset { get { return "Terrain"; } }
			public static string TerrainImageFile { get{ return "map.bmp"; } }
			public static string TerrainModuleFile { get { return "module.json"; } }

			public static string Database { get{return "WarWorldInfDB";} }
			public static string DbServer { get{return "localhost";} }
			public static string DbUsername { get {return "root";} }
			public static string DbPassword { get{return "";} }
		}

		public static int SessionKeyLength { get; private set; }
		public static int ServerPort { get; private set; }
		public static int ClientPort { get; private set; }
		public static int SecondsInTicks { get; private set; }
		public static int UserTimeoutTime { get; private set;}
		public static string WorldSaveFile { get; private set; }
		public static int AutoSaveInterval { get; private set; }
        public static int MinOpDistance { get; private set; }
        public static int BaseRadarRadius { get; private set; }

        public static int TerrainWidth { get; private set; }
		public static int TerrainHeight { get; private set; }
		public static int TerrainSeed { get; private set; }
		public static string TerrainPreset { get; private set; }
		public static string TerrainImageFile { get; private set; }
        public static string TerrainModuleFile { get; private set; }

		public static string Database { get; private set; }
		public static string DbServer { get; private set; }
		public static string DbUsername { get; private set; }
        public static string DbPassword { get; private set; }

		private IniData _config;

		public IniData Config{ get { return _config; } }
	
		public AppSettings (string file)
		{
			FileIniDataParser parser = new FileIniDataParser();
			parser.Parser.Configuration.CommentString = "#";
			if (File.Exists (file)) {
				_config = parser.ReadFile (file);
				LoadSettings();
			} else
				Logger.LogError ("Config file not found!");
		}

		public void LoadSettings(){
			SessionKeyLength = TryGetValue("General", "SessionKeyLength", Defaults.SessionKeyLength);
			ServerPort = TryGetValue("General", "ServerPort", Defaults.ServerPort);
			ClientPort = TryGetValue("General", "ClientPort", Defaults.ClientPort);
			SecondsInTicks = TryGetValue("General", "SecondsInTicks", Defaults.SecondsInTicks);
			UserTimeoutTime = TryGetValue("General", "UserTimeoutTime", Defaults.UserTimeoutTime);
			WorldSaveFile = TryGetValue("General", "WorldSaveFile", Defaults.WorldSaveFile);
			AutoSaveInterval = TryGetValue("General", "AutoSaveInterval", Defaults.AutoSaveInterval);
            MinOpDistance = TryGetValue("General", "MinOpDistance", Defaults.MinOpDistance);
            BaseRadarRadius = TryGetValue("General", "BaseRadarRadius", Defaults.BaseRadarRadius);

            TerrainWidth = TryGetValue("Terrain", "Width", Defaults.TerrainWidth);
			TerrainHeight = TryGetValue("Terrain", "Height", Defaults.TerrainHeight);
			TerrainSeed = TryGetValue("Terrain", "Seed", Defaults.TerrainSeed);
			TerrainPreset = TryGetValue("Terrain", "Preset", Defaults.TerrainPreset);
			TerrainImageFile = TryGetValue("Terrain", "ImageFile", Defaults.TerrainImageFile);
			TerrainModuleFile = TryGetValue("Terrain", "ModuleFile", Defaults.TerrainModuleFile);

			Database = TryGetValue("Database", "Database", Defaults.Database);
			DbServer = TryGetValue("Database", "Server", Defaults.DbServer);
			DbUsername = TryGetValue("Database", "Username", Defaults.DbUsername);
			DbPassword = TryGetValue("Database", "Password", Defaults.DbPassword);
		}

		public T GetSettingValue<T>(string section, string setting){
			try {
				Type typeT = typeof(T);
				if (_config != null) {
					if (_config.Sections.ContainsSection (section)) {
						if (_config [section].ContainsKey (setting)) {
							string value = _config [section] [setting];
							return (T)Convert.ChangeType(value, typeT);
						} else
							Logger.LogError ("Setting \"{0}\" does not exist in section \"{1}\".", setting, section);
					} else
						Logger.LogError ("Section \"{0}\" does not exist.", section);
				}
			}
			catch(Exception e){
				Logger.Log("Failed loading {0}.{1}: {2}", section, setting, e.Message);
			}
			return default(T);
		}

		public bool ContainsSection(string section){
			return _config != null && _config.Sections.ContainsSection (section);
		}

		public bool ContainsSetting(string section, string setting){
			return _config != null && _config.Sections.ContainsSection (section) && _config[section].ContainsKey (setting);
		}

		private T TryGetValue<T>(string section, string setting, T defaultValue){
			return ContainsSetting (section, setting) ? GetSettingValue<T> (section, setting) : defaultValue;
		}
	}
}

