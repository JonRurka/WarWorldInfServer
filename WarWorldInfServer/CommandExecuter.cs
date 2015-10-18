using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;

namespace WarWorldInfServer
{
	public class CommandExecuter
	{
		public delegate object CommandFunction(params string[] args);

		private Dictionary<string, CommandFunction> _cmdTable = new Dictionary<string, CommandFunction>();
		private Dictionary<string, CommandDescription> _cmdDescription = new Dictionary<string, CommandDescription>();

		private string _input = string.Empty;
		private GameServer _server;
		private int padAmount = 0;

		public CommandExecuter ()
		{
			_server = GameServer.Instance;
		}

		public void StartCommandLoop(){
			//Logger.Log ("Command Executer initialized.");
			LoadCommands ();
			while (_server.Running) {

				try {
					ConsoleKeyInfo key = Console.ReadKey(true);
					if (key.Key == ConsoleKey.Backspace){
						if (_input.Length > 0)
							_input = _input.Remove(_input.Length - 1);
					}
					else if (key.Key == ConsoleKey.Enter){
						string tmpStr = _input;
						_input = string.Empty;
						TaskQueue.QueueMain(()=>ExecuteCommand(tmpStr));
					}
					else{
						_input += key.KeyChar;
					}
					Logger.InputStr = _input;
				}
				catch (Exception e){
					Logger.LogError("Error in command loop.");
				}
			}
		}

		public void LoadCommands(){
			CommandDescription[] commands = ConfigParser.GetCommands (GameServer.Instance.AppDirectory + "Commands.ini");
			for (int i = 0; i < commands.Length; i++) {
				RegisterCommand(commands[i]);
				if (commands[i].command.Length > padAmount)
					padAmount = commands[i].command.Length;
			}
		}

		public void RegisterCommand(CommandDescription command){
			_cmdTable [command.command.ToLower ()] = command.callback;
			_cmdDescription [command.command.ToLower ()] = command;
		}

		public void UnregisterCommand(string commandString){
			_cmdTable.Remove (commandString.ToLower ());
			_cmdDescription.Remove (commandString.ToString ());
		}

		public string[] Commands(){
			string[] commands = new string[_cmdTable.Keys.Count];
			_cmdTable.Keys.CopyTo (commands, 0);
			return commands;
		}

		private void ExecuteCommand(string command){
			Logger.Print ("> {0}", command);
			command = command.Trim ();
			if (!string.IsNullOrEmpty (command)) {
				string[] args = command.Split (new[]{' '}, System.StringSplitOptions.RemoveEmptyEntries);
				string cmd = args [0].ToLower ();
				if (_cmdTable.ContainsKey (cmd)) {
					string result = _cmdTable [cmd] (args).ToString ();
					if (result != string.Empty)
						Logger.Print (result);
				} else
					Logger.LogError ("Command not found: {0}", args [0]);
			}
		}

		// command functions

		private object Runtime_CMD(params string[] args){
			if (_server.WorldLoaded)
				return _server.GameTime.GetRuntime();
			Logger.LogWarning ("World not loaded.");
			return string.Empty;
		}

		private object Time_CMD(params string[] args){
			Logger.Log (GameTimer.GetDateTime ());
			return string.Empty;
		}

		private object Create_CMD(params string[] args){
			if (args.Length == 2){
				_server.Worlds.CreateWorld(args[1]);
			}
			return string.Empty;
		}

		private object Load_CMD(params string[] args){
			if (args.Length == 2){
				_server.Worlds.LoadWorld(args[1]);
			}
			return string.Empty;
		}

		private object Save_CMD(params string[] args){
			if (args.Length == 1)
				_server.Save ();
			else if (args.Length == 2)
				_server.Save (args [0]);
			return string.Empty;
		}

		private object Clear_CMD(params string[] args){
			Logger.Clear ();
			return string.Empty;
		}

		private object Exit_CMD(params string[] args){
			if (_server.WorldLoaded)
				_server.Worlds.SaveCurrentWorld ();
			_server.Exit ();
			return "Good bye...";
		}

		private object Help_CMD(params string[] args){
			StringBuilder output = new StringBuilder ();
			if (args.Length == 1) {
				output.AppendLine(string.Format("Commands ({0}):", _cmdTable.Count.ToString()));
				foreach (string key in _cmdTable.Keys) {
					output.AppendLine (string.Format ("  {0} : {1}", key.PadRight(padAmount, ' '), _cmdDescription [key].description_small));
				}
			} else if (args.Length == 2) {
				if (_cmdTable.ContainsKey (args [1])) {
					output.AppendLine(string.Format(" - Command: {0} {1}", _cmdDescription [args [1]].command, _cmdDescription [args [1]].command_args));
					output.AppendLine(string.Format (" - Short description: {0}", _cmdDescription [args [1]].description_small));
					if (!string.IsNullOrEmpty(_cmdDescription [args [1]].description_Long))
						output.AppendLine( string.Format(" - Long description: {0}", _cmdDescription [args [1]].description_Long));
				} else
					Logger.LogError ("Command \"{0}\" not found", args [1]);
			} else {
				Logger.LogError("To many arguments.");
			}
			return output.ToString();
		}

		private object preview_CMD(params string[] args){
			SettingsLoader settings = GameServer.Instance.Settings;
			int seed = 0;
			if (args.Length == 1)
				seed = settings.TerrainSeed;
			else if (args.Length == 2 && args [1].Equals ("-r")) {
				seed = new Random (GameTimer.GetEpoch ()).Next (int.MinValue, int.MaxValue);
			} else if (args.Length == 2) {
				if (int.TryParse(args[1], out seed));
			}
			Thread thread = new Thread(()=>{
				TerrainBuilder builder = new TerrainBuilder(settings.TerrainWidth, settings.TerrainHeight, seed);
				builder.Generate (LibNoise.GradientPresets.Terrain);
				System.Drawing.Bitmap map = builder.GetBitmap();
				Form imageForm = new Form ();
				imageForm.Text = "Seed preview: " + seed;
				imageForm.Width = settings.TerrainWidth;
				imageForm.Height = settings.TerrainHeight;
				imageForm.FormBorderStyle = FormBorderStyle.Sizable;
				imageForm.Controls.Add (new PictureBox () {Image = map, Dock = DockStyle.Fill});
				imageForm.ShowDialog();
				Logger.Log("Preview closed.");
				imageForm.Close();
				imageForm.Dispose();
			});
			thread.Start ();
			return "Seed: " + seed;
		}
	}
}

