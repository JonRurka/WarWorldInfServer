using System;
using System.IO;

namespace WarWorldInfServer
{
	public class CommandExecuter
	{
		string input = string.Empty;
		GameServer _server;
		public CommandExecuter ()
		{
			_server = GameServer.Instance;
		}

		public void StartCommandLoop(){
			while (_server.Running) {
				ConsoleKeyInfo key = Console.ReadKey(true);
				if (key.Key == ConsoleKey.Backspace){
					if (input.Length > 0)
						input = input.Remove(input.Length - 1);
				}
				else if (key.Key == ConsoleKey.Enter){
					ExecuteCommand(input);
					input = string.Empty;
				}
				else{
					input += key.KeyChar;
				}
				Logger.InputStr = input;
			}
		}

		public void ExecuteCommand(string command){
			Logger.Print ("> {0}", command);
			string[] args = command.Split(' ');
			if (args.Length > 0){
				switch(args[0])
				{
				case "runtime":
					Logger.Log(_server.GameTime.GetRuntime());
					break;
				case "time":
					Logger.Print(GameTimer.GetDateTime());
					break;
				case "create":
					if (args.Length == 2){
						_server.WorldMnger.CreateWorld(args[1]);
					}
					break;
				case "load":
					if (args.Length == 2){
						_server.WorldMnger.LoadWorld(args[1]);
					}
					break;
				case "save":
					if (args.Length == 1)
						_server.WorldMnger.SaveCurrentWorld();
					else if (args.Length == 2)
						_server.WorldMnger.SaveWorld(args[1]);
					break;
				default:
					Logger.LogError("Command not found: {0}", args[0]);
					break;
				}
			}
		}
	}
}

