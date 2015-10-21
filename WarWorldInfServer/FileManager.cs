using System;
using System.IO;
using Newtonsoft.Json;

namespace WarWorldInfServer
{
	public static class FileManager
	{
		public static void SaveConfigFile(string file, object saveObject, bool saveTypes){
			string jsonStr = string.Empty;
			if (saveTypes)
				jsonStr = JsonConvert.SerializeObject (saveObject, Formatting.Indented, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.All});
			else 
				jsonStr = JsonConvert.SerializeObject (saveObject, Formatting.Indented);
			SaveString (file, jsonStr);
		}

		public static T LoadObject<T>(string file, bool loadTypes){
			string contents =  LoadString(file);
			T jsonObj;
			if (loadTypes)
				jsonObj = JsonConvert.DeserializeObject<T> (contents, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.All});
			else
				jsonObj = JsonConvert.DeserializeObject<T> (contents);
			return jsonObj;
		}

		public static void SaveString(string file, string content){
			StreamWriter writer = File.CreateText (file);
			writer.Write (content);
			writer.Close ();
		}

		public static string LoadString(string file){
			string content = string.Empty;
			StreamReader reader = File.OpenText (file);
			content = reader.ReadToEnd ();
			reader.Close ();
			return content;
		}
	}
}

