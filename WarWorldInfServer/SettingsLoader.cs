using System;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using IniParser;
using IniParser.Parser;
using IniParser.Model;


namespace WarWorldInfServer
{
	public class SettingsLoader
	{
		public static class StandardSettingsDefaults{
			public static int SessionKeyLength { get { return 32; } }
			public static int ServerPort { get { return 9999; } }
			public static int ClientPort { get { return 9999; } }
			public static int SecondsInTicks { get { return 3600; } }
			public static int UserTimeoutTime { get { return 3600; } }
			public static string WorldSaveFile { get { return "WorldSave.json"; } }
			public static int AutoSaveInterval { get { return 300; } }

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

		public int SessionKeyLength { get; private set; }
		public int ServerPort { get; private set; }
		public int ClientPort { get; private set; }
		public int SecondsInTicks { get; private set; }
		public int UserTimeoutTime { get; private set;}
		public string WorldSaveFile { get; private set; }
		public int AutoSaveInterval { get; private set; }
		
		public int TerrainWidth { get; private set; }
		public int TerrainHeight { get; private set; }
		public int TerrainSeed { get; private set; }
		public string TerrainPreset { get; private set; }
		public string TerrainImageFile { get; private set; }
		public string TerrainModuleFile { get; private set; }

		public string Database { get; private set; }
		public string DbServer { get; private set; }
		public string DbUsername { get; private set; }
		public string DbPassword { get; private set; }

		private IniData _config;

		public IniData Config{ get { return _config; } }
	
		public SettingsLoader (string file)
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
			SessionKeyLength = TryGetValue<int>("General", "SessionKeyLength", StandardSettingsDefaults.SessionKeyLength);
			ServerPort = TryGetValue<int>("General", "ServerPort", StandardSettingsDefaults.ServerPort);
			ClientPort = TryGetValue<int>("General", "ClientPort", StandardSettingsDefaults.ClientPort);
			SecondsInTicks = TryGetValue<int>("General", "SecondsInTicks", StandardSettingsDefaults.SecondsInTicks);
			UserTimeoutTime = TryGetValue<int>("General", "UserTimeoutTime", StandardSettingsDefaults.UserTimeoutTime);
			WorldSaveFile = TryGetValue<string>("General", "WorldSaveFile", StandardSettingsDefaults.WorldSaveFile);
			AutoSaveInterval = TryGetValue<int>("General", "AutoSaveInterval", StandardSettingsDefaults.AutoSaveInterval);
			
			TerrainWidth = TryGetValue<int>("Terrain", "Width", StandardSettingsDefaults.TerrainWidth);
			TerrainHeight = TryGetValue<int>("Terrain", "Height", StandardSettingsDefaults.TerrainHeight);
			TerrainSeed = TryGetValue<int>("Terrain", "Seed", StandardSettingsDefaults.TerrainSeed);
			TerrainPreset = TryGetValue<string>("Terrain", "Preset", StandardSettingsDefaults.TerrainPreset);
			TerrainImageFile = TryGetValue<string>("Terrain", "ImageFile", StandardSettingsDefaults.TerrainImageFile);
			TerrainModuleFile = TryGetValue<string>("Terrain", "ModuleFile", StandardSettingsDefaults.TerrainModuleFile);

			Database = TryGetValue<string>("Database", "Database", StandardSettingsDefaults.Database);
			DbServer = TryGetValue<string>("Database", "Server", StandardSettingsDefaults.DbServer);
			DbUsername = TryGetValue<string>("Database", "Username", StandardSettingsDefaults.DbUsername);
			DbPassword = TryGetValue<string>("Database", "Password", StandardSettingsDefaults.DbPassword);
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

