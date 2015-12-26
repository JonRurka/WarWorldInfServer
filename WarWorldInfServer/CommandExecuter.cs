using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using WarWorldInfinity.Shared;
using WarWorldInfinity.LibNoise;

namespace WarWorldInfinity
{
	public class CommandExecuter
	{
		public delegate object CommandFunction(User caller, params string[] args);

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
						TaskQueue.QueueMain(()=>ExecuteCommand( "_server_",tmpStr));
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

		public string ExecuteCommand(string player, string command){
            string result = string.Empty;
            try {
				Logger.Print ("> {0}", command);
				command = command.Trim ();
				if (!string.IsNullOrEmpty (command)) {
					string[] args = command.Split (new[]{' '}, System.StringSplitOptions.RemoveEmptyEntries);
					string cmd = args [0].ToLower ();
                    User user = _server.Users.GetUser(player);
                    if (_cmdTable.ContainsKey(cmd)) {
                        result = _cmdTable[cmd](user, args).ToString();
                        if (result != string.Empty)
                            Logger.Print(result);
                    }
                    else {
                        Logger.LogError("Command not found: {0}", args[0]);
                        result = "Command not found: " + args[0];
                    }
				}
			}
			catch (Exception e){
				Logger.LogError("{0}\n{1}", e.Message, e.StackTrace);
                result = "error: " + e.Message;
			}
            return result;
		}

        public bool CommandExists(string command) {
            return _cmdDescription.ContainsKey(command);
        }

        public CommandDescription GetCommandDescription(string command) {
            if (CommandExists(command))
                return _cmdDescription[command];
            return default(CommandDescription);
        }

		// command functions

		private object Runtime_CMD(User caller, params string[] args){
			if (_server.WorldLoaded)
				return _server.GameTime.GetRuntime();
			Logger.LogWarning ("World not loaded.");
			return string.Empty;
		}

		private object Time_CMD(User caller, params string[] args){
			return GameTimer.GetDateTime();
		}

		private object Create_CMD(User caller, params string[] args){
			try {
				if (args.Length == 2){
					TaskQueue.QeueAsync("GeneralWorker", ()=> _server.Worlds.CreateWorld(args[1]));
				}
			}
			catch(Exception e){
				Logger.LogError("{0}\n{1}", e.Message, e.StackTrace);
			}
			return string.Empty;
		}

		private object Load_CMD(User caller, params string[] args){
			try {
				if (args.Length == 2){
					_server.Worlds.LoadWorld(args[1]);
				}
			}
			catch(Exception e){
				Logger.LogError("{0}: {1}\n{2}", e.GetType().ToString(), e.Message, e.StackTrace);
			}
			return string.Empty;
		}

		private object Save_CMD(User caller, params string[] args){
			if (args.Length == 1)
				_server.Save ();
			else if (args.Length == 2)
				_server.Save (args [1]);
			return string.Empty;
		}

		private object Clear_CMD(User caller, params string[] args){
			Logger.Clear ();
			return string.Empty;
		}

		private object Exit_CMD(User caller, params string[] args){
			if (_server.WorldLoaded)
				_server.Worlds.SaveCurrentWorld ();
			_server.Exit ();
			return "Good bye...";
		}

		private object Help_CMD(User caller, params string[] args){
			StringBuilder output = new StringBuilder ();
			if (args.Length == 1) {
				output.AppendLine(string.Format("Commands ({0}):", _cmdTable.Count.ToString()));
				foreach (string key in _cmdTable.Keys) {
                    if (caller.CanCallCommand(key))
					    output.AppendLine (string.Format ("  {0} : {1}", key.PadRight(padAmount, ' '), _cmdDescription [key].description_small));
				}
			} else if (args.Length == 2) {
				if (_cmdTable.ContainsKey (args [1])) {
                    if (caller.CanCallCommand(args[1])) {
                        output.AppendLine(string.Format(" - Command: {0} {1}", _cmdDescription[args[1]].command, _cmdDescription[args[1]].command_args));
                        output.AppendLine(string.Format(" - Short description: {0}", _cmdDescription[args[1]].description_small));
                        if (!string.IsNullOrEmpty(_cmdDescription[args[1]].description_Long))
                            output.AppendLine(string.Format(" - Long description: {0}", _cmdDescription[args[1]].description_Long));
                    }
                    else
                        return string.Format("error: {0} permissions required.", _cmdDescription[args[1]].permission.ToString());
                } else
                    return "Command not found: " + args[1];

			} else {
				return "To many arguments.";
			}
			return output.ToString();
		}

		private object Preview_CMD(User caller, params string[] args){
			int seed = 0;
			if (args.Length == 1)
				seed = AppSettings.TerrainSeed;
			else if (args.Length == 2 && args [1].Equals ("-r")) {
				seed = new Random (GameTimer.GetEpoch ()).Next (int.MinValue, int.MaxValue);
			} else if (args.Length == 2) {
				if (int.TryParse(args[1], out seed));
			}
			TaskQueue.QeueAsync("terrain preview", ()=>{
				try {
					List<GradientPresets.GradientKeyData> keys = new List<GradientPresets.GradientKeyData>();
                    keys.Add(new GradientPresets.GradientKeyData(new Color(255, 0, 0, 128), 0));
					keys.Add(new GradientPresets.GradientKeyData(new Color(255, 32, 64, 128), 0.4f));
					keys.Add(new GradientPresets.GradientKeyData(new Color(255, 64, 96, 191), 0.48f));
					keys.Add(new GradientPresets.GradientKeyData(new List<string> {"sand.png"}, 0.5f));
					keys.Add(new GradientPresets.GradientKeyData(new List<string> {"trees1.png", "trees2.png" }, 0.52f));
					keys.Add(new GradientPresets.GradientKeyData(new List<string> {"grass1.png", "trees2.png"}, 0.55f));
					keys.Add(new GradientPresets.GradientKeyData(new List<string> {"grass1.png", "grass2.png", "trees2.png"}, 0.60f));
					keys.Add(new GradientPresets.GradientKeyData(new List<string> {"grass1.png", "grass2.png"}, 0.90f));
					keys.Add(new GradientPresets.GradientKeyData(new List<string> {"grass1.png", "grass2.png"}, 1f));
					//GradientPresets.CreateGradient(keys);
					GradiantPresetLoader.PresetSerializer saveObj = new GradiantPresetLoader.PresetSerializer(AppSettings.TerrainPreset, keys);
					FileManager.SaveConfigFile(GameServer.Instance.AppDirectory + "GradientPresets" + GameServer.sepChar + AppSettings.TerrainPreset+".json", saveObj, false);
					//return;
					IModule module = new Perlin ();
					((Perlin)module).OctaveCount = 16;
					((Perlin)module).Seed = AppSettings.TerrainSeed;

					TerrainBuilder builder = new TerrainBuilder(AppSettings.TerrainWidth, AppSettings.TerrainHeight, seed);
					builder.Generate (module, AppSettings.TerrainPreset);
					System.Drawing.Bitmap map = builder.GetBitmap();
                    /*System.Windows.Forms.Form imageForm = new System.Windows.Forms.Form();
					imageForm.Text = "Seed preview: " + seed;
					imageForm.Width = settings.TerrainWidth;
					imageForm.Height = settings.TerrainHeight;
					imageForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
					imageForm.Controls.Add (new System.Windows.Forms.PictureBox () {Image = map, Dock = System.Windows.Forms.DockStyle.Fill});
					imageForm.ShowDialog();
					Logger.Log("Preview closed.");
					imageForm.Close();
					imageForm.Dispose();*/
				}
				catch(Exception e){
					Logger.LogError("{0}\n{1}", e.Message, e.StackTrace);
				}
			});
			return "Seed: " + seed;
		}

		private object NetUsage_CMD(User caller, params string[] args){
			_server.Net.DisplayDataRate ();
			return string.Empty;
		}

        private object NewUser_CMD(User caller, params string[] args) {
            if (args.Length == 5)
            {
                string username = args[1];
                string password = args[2];
                string permission = args[3];
                string email = args[4];
                return GameServer.Instance.CreatePlayer(username, password, permission, email);
            }
            else
                return "Requires 4 arguments.";
        }

        private object NewAlliance(User caller, params string[] args){
            if (caller.Permission == User.PermissionLevel.Server) {
                if (args.Length == 2) {
                    GameServer.Instance.A
                }
            }
            else if (caller.Permission == User.PermissionLevel.Admin) {
                
            } 
               
        }
	}
}

