using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using IniParser;
using IniParser.Parser;
using IniParser.Model;

namespace WarWorldInfinity
{
	public static class ConfigParser {
		public static CommandDescription[] GetCommands(string file) {
			List<CommandDescription> commands = new List<CommandDescription>();
			FileIniDataParser parser = new FileIniDataParser();
			parser.Parser.Configuration.CommentString = "#";
			if (File.Exists (file)) {
				IniData config = parser.ReadFile (file);
				foreach (SectionData section in config.Sections) {
					string command = section.SectionName;
					string command_args = string.Empty;
					string description_small = string.Empty;
					string description_Long = string.Empty;
                    User.PermissionLevel permission = User.PermissionLevel.Server;
					string callback = string.Empty;
				
					if (config [command].ContainsKey ("command_args")) {
						command_args = config [command] ["command_args"];
					}
				
					if (config [command].ContainsKey ("description_small")) {
						description_small = config [command] ["description_small"];
					} else {
						Logger.LogError ("Failed to parse Command \"" + command + "\": Short description not specified.");
					}
				
					if (config [command].ContainsKey ("description_Long")) {
						description_Long = config [command] ["description_Long"];
					}
				
                    if (config[command].ContainsKey("permission")) {
                        string permissionStr = config[command]["permission"];
                        User.PermissionLevel tmpPerm;
                        if (Enum.TryParse(permissionStr, true, out tmpPerm)) 
                            permission = tmpPerm;
                    }

					if (config [command].ContainsKey ("function")) {
						callback = config [command] ["function"];
					} else {
						Logger.LogError ("Failed to parse Command \"" + command + "\": Function not specified.");
					}
				
					commands.Add (new CommandDescription (command, command_args, description_small, description_Long, permission, callback));
				}
			} else
				Logger.LogError("Command file not found.");
			return commands.ToArray();
		}
		
		public static IniData GetConfig(string file) {
			FileIniDataParser parser = new FileIniDataParser();
			parser.Parser.Configuration.CommentString = "#";
			return parser.ReadFile(file);
		}
		
		public static string GetValue(string file, string section, string key) {
			IniData config = GetConfig(file);
			if (config.Sections.ContainsSection(section) && config[section].ContainsKey(key)) {
				return config[section][key];
			}
			else return string.Empty;
		}
		
		public static string GetValue(IniData config, string section, string key) {
			if (config.Sections.ContainsSection(section) && config[section].ContainsKey(key)) {
				return config[section][key];
			}
			else return string.Empty;
		}
	}
}

