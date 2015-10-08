using System;
using System.IO;
using Newtonsoft.Json;

namespace WarWorldInfServer
{
	public static class FileManager
	{
		public static void SaveConfigFile(string file, object saveObject){
			string jsonStr = JsonConvert.SerializeObject (saveObject);
			SaveString (file, jsonStr);
		}

		public static object LoadObject<T>(string file){
			string contents =  LoadString(file);
			return JsonConvert.DeserializeObject<T> (contents);
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

