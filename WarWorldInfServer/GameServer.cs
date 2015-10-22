using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using LibNoise;
using LibNoise.Models;
using LibNoise.Modifiers;


namespace WarWorldInfServer
{
	public class GameServer
	{
		private readonly string[] _commandArgs;
		private readonly string _directory;
		
		private Thread _tickThread;
		private ManualResetEvent _resetEvent = new ManualResetEvent(false);
		private TaskQueue _taskQueue;
		private NetworkCommands _netCommands;
	  
		public static GameServer Instance{ get; private set; }

		public string Version{ get { return "0.0.1-dev"; } }
		public string[] CommandArgs{ get { return _commandArgs; } }
		public string AppDirectory { get { return _directory; } }
		public bool Running { get; private set; }
		public bool WorldLoaded { get; private set; }
		public CommandExecuter CommandExec{ get; private set; }
		public GameTimer GameTime{ get; private set; }
		public WorldManager Worlds { get; private set; }
		public SettingsLoader Settings { get; private set; }
		public UserManager Users { get; private set; }
		public NetServer Net { get; private set; }
		public DataBase DB { get; private set; }

		public GameServer (string[] args)
		{
			_commandArgs = args;
			_directory = Directory.GetCurrentDirectory () + "/";
			GradientPresets.AppDirectory = _directory;
			Running = true;
			Instance = this;
			_resetEvent = new ManualResetEvent(false);
		}

		public void Run(){
			Settings = new SettingsLoader (_directory + "Settings.ini");
			_taskQueue = new TaskQueue ();
			CommandExec = new CommandExecuter ();
			Logger.Log ("War World Infinity Server Version {0}", Version);
			Net = new NetServer (Settings.ServerPort, Settings.ClientPort);
			_netCommands = new NetworkCommands();
			DB = new DataBase (Settings.DbServer, Settings.Database);
			DB.Connect (Settings.DbUsername, Settings.DbPassword);
			Worlds = new WorldManager (this);
			Users = new UserManager ();
			_tickThread = new Thread (CommandExec.StartCommandLoop);
			_tickThread.Start ();
			GameLoop ();
		}

		public void GameLoop(){
			//Logger.Log (GetMonoRuntime ());
			while (Running) {
				_resetEvent.WaitOne (1);
				try {
					if (WorldLoaded){
						if (GameTime.Update()){ // every second
							if (GameTime.Seconds % Settings.AutoSaveInterval == 0)
								Save();
							if (GameTime.TickIncrease){ // update when enough seconds pass to increase tick.
								Logger.Log("Updating world. Tick: {0}", GameTime.Tick.ToString());
								Save();
							}
						}
					}
					Net.Update();
					_taskQueue.Update(); // run items queued during frame.
					Logger.Update(); // print items sent to log during frame.
				}
				catch (Exception e){
					Logger.LogError("{0}: {1}\n{1}", e.InnerException.GetType(), e.Message, e.StackTrace);
				}
			}
		}



		public void CreatePlayer(string username, string password, string permission, string email){
			string salt = HashHelper.RandomKey(32);
			DB.AddNewPlayer (username, HashHelper.HashPasswordFull(password, salt), salt, permission, email);
			Logger.Log ("User {0} created as {1}.", username, permission);
		}

		public void StartWorld(World world){
			GameTime = new GameTimer (world.WorldStartTime);
			WorldLoaded = true;
			Logger.Log ("World \"{0}\" started.", world.WorldName);
		}

		public void Save(){
			Save (Worlds.CurrentWorld.WorldName);
		}

		public void Save(string world){
			Worlds.SaveWorld (world);
		}

		public void Exit(){
			WorldLoaded = false;
			Net.Close();
			TaskQueue.Close ();
			Running = false;
			_tickThread.Abort ();
		}

		private Boolean IsMonoRuntime()
		{
			return (Type.GetType("Mono.Runtime") != null);
		}

		private String GetMonoRuntime()
		{
			Type type = Type.GetType("Mono.Runtime");
			
			if (type != null)
			{
				MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
				if (displayName == null)
					return "Unix/Linux + Mono";
				else
					return "Unix/Linux + Mono " + displayName.Invoke(null, null);
			}
			
			return String.Empty;
		}
	}
}

