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
			//TaskQueue.QueueMain(()=>LogToFile (message.ToString()));
			lock (_messages) {
				_messages.Add (string.Format (message.ToString(), args));
			}
			TaskQueue.QueueMain(()=>LogToFile (string.Format(message.ToString(), args)));

		}

		public static void PrintNoFormat(object message){
			lock (_messages) {
				_messages.Add (message.ToString());
			}
			TaskQueue.QueueMain(()=>LogToFile (message.ToString()));
		}

		public static void Log(object message, params object[] args){
			Print (string.Format("[{0}]: {1}", GameTimer.GetTime(), string.Format(message.ToString(), args)));
		}

		public static void LogWarning(object message, params object[] args){
			Print (string.Format("[{0} WARNING]: {1}", GameTimer.GetTime(), string.Format(message.ToString(), args)));
		}

		public static void LogError(object message, params object[] args){
			Print (string.Format("[{0} ERROR]: {1}", GameTimer.GetTime(), string.Format(message.ToString(), args)));
		}

		private static List<string> _messages = new List<string>();
		private static List<string> _currentmessages = new List<string>();

		public static void Clear(){
			_currentmessages.Clear ();
			InputStr = string.Empty;
		}

		public static void Update(){
			if (_messages.Count > 0 || _letterTyped) {
				Draw();
			}
		}

		public static void LogToFile(object message){
			if (!Directory.Exists (Directory.GetCurrentDirectory () + "/SystemLogs/"))
				Directory.CreateDirectory (Directory.GetCurrentDirectory () + "/SystemLogs/");

			if (_logFile == string.Empty) {
				int fileCount = Directory.GetFiles(Directory.GetCurrentDirectory () + "/SystemLogs/").Length;
				_logFile = Directory.GetCurrentDirectory () + "/SystemLogs/" + fileCount + ". " + GameTimer.GetDateTime ().Replace ("/", "-") + ".txt";
			}
			StreamWriter writer = new StreamWriter (_logFile, true);
			writer.WriteLine(message.ToString());
			writer.Close ();
		}

		private static void Draw(){
			_currentmessages.AddRange (_messages);
			_messages.Clear ();
			Console.Clear ();
			for (int i = 0; i < _currentmessages.Count; i++) {
				Console.WriteLine (_currentmessages [i]);
			}
			Console.Write("> {0}", InputStr);
			_letterTyped = false;
		}
	}
}

