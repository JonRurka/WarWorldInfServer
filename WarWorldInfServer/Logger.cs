using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace WarWorldInfServer
{
	public static class Logger
	{
		private static bool _letterTyped = false;
		private static string _inputStr = string.Empty;
		private static string _logFile = string.Empty;
		public static string LogFile { get { return _logFile; } }
		public static string InputStr { 
			get { 
				return _inputStr; 
			} 
			set { 
				_inputStr = value;
				_letterTyped = true; 
			} 
		}

		public static void Print(object message, params object[] args){
			lock (_messages) {
				_messages.Add(string.Format(message.ToString(), args));
			}
			LogToFile (string.Format(message.ToString(), args));
		}

		public static void Log(object message, params string[] args){
			Print (string.Format("[{0}]: {1}", GameTimer.GetTime(), string.Format(message.ToString(), args)));
		}

		public static void LogWarning(object message, params string[] args){
			Print (string.Format("[{0} WARNING]: {1}", GameTimer.GetTime(), string.Format(message.ToString(), args)));
		}

		public static void LogError(object message, params string[] args){
			Print (string.Format("[{0} ERROR]: {1}", GameTimer.GetTime(), string.Format(message.ToString(), args)));
		}

		private static List<string> _messages = new List<string>();
		private static List<string> _currentmessages = new List<string>();

		public static void Update(){
			//_currentActions.Clear ();
			if (_messages.Count > 0 || _letterTyped) {
				_currentmessages.AddRange (_messages);
				_messages.Clear ();
				if (_currentmessages.Count > 0) {
					Console.Clear ();
					for (int i = 0; i < _currentmessages.Count; i++) {
						Console.WriteLine (_currentmessages [i]);
					}
					Console.Write("> {0}", InputStr);
					_letterTyped = false;
				}
			}
		}

		public static void LogToFile(object message){
			if (!Directory.Exists (Directory.GetCurrentDirectory () + "/SystemLogs/"))
				Directory.CreateDirectory (Directory.GetCurrentDirectory () + "/SystemLogs/");
			//Console.WriteLine (Directory.GetCurrentDirectory () + "/Logs/");

			if (_logFile == string.Empty) 
				_logFile = Directory.GetCurrentDirectory() + "/SystemLogs/" + GameTimer.GetDateTime().Replace("/","-") + ".txt";
			//Console.WriteLine (_logFile);
			//FileStream stream = File.Open(LogFile, FileMode.OpenOrCreate, FileAccess.Write);
			StreamWriter writer = new StreamWriter (_logFile, true);
			writer.WriteLine(message.ToString());
			writer.Close ();
		}
	}
}

