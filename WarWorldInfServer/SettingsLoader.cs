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
			public static int TerrainWidth { get { return 1920; } }
			public static int TerrainHeight { get { return 1080; } }
			public static int TerrainSeed { get { return 0; } }
			public static string TerrainImageFile { get{ return "map.bmp"; } }
		}

		public class StandardSettings{
			private SettingsLoader _loader;
			public int TerrainWidth { get; private set; }
			public int TerrainHeight { get; private set; }
			public int TerrainSeed { get; private set; }
			public string TerrainImageFile { get; private set; }

			public StandardSettings(SettingsLoader loader){
				_loader = loader;
				TerrainWidth = loader.TryGetValue<int>("Terrain", "Width", StandardSettingsDefaults.TerrainWidth);
				TerrainHeight = loader.TryGetValue<int>("Terrain", "Height", StandardSettingsDefaults.TerrainHeight);
				TerrainSeed = loader.TryGetValue<int>("Terrain", "Seed", StandardSettingsDefaults.TerrainSeed);
				TerrainImageFile = loader.TryGetValue<string>("Terrain", "ImageFile", StandardSettingsDefaults.TerrainImageFile);
			}
		}

		private IniData _config;
		private StandardSettings _standard;

		public IniData Config{ get { return _config; } }
		public StandardSettings Standard { get { return _standard; } }
	
		public SettingsLoader (string file)
		{
			FileIniDataParser parser = new FileIniDataParser();
			parser.Parser.Configuration.CommentString = "#";
			if (File.Exists (file)) {
				_config = parser.ReadFile (file);
				_standard = new StandardSettings(this);
			} else
				Logger.LogError ("Config file not found!");
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

