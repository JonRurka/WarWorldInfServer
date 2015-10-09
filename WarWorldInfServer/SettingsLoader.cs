using System;
using System.IO;
using System.ComponentModel;
using IniParser;
using IniParser.Parser;
using IniParser.Model;


namespace WarWorldInfServer
{
	public class SettingsLoader
	{
		private IniData config;

		public string Version {
			get {
				if (config != null)
					return GetSettingValue<string>("General", "Version");
				return string.Empty;
			}
		}

		public SettingsLoader (string file)
		{
			FileIniDataParser parser = new FileIniDataParser();
			parser.Parser.Configuration.CommentString = "#";
			if (File.Exists (file)) {
				config = parser.ReadFile (file);
			} else
				Logger.LogError ("Config file not find!");
		}

		public T GetSettingValue<T>(string section, string setting){
			try {
				Type typeT = typeof(T);
				if (config != null) {
					if (config.Sections.ContainsSection (section)) {
						if (config [section].ContainsKey (setting)) {
							string value = config [section] [setting];
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
	}
}

