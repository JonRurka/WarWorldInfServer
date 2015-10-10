using System;
using System.IO;
using System.Threading;

namespace WarWorldInfServer
{
	public class GameServer
	{
		private readonly string[] _commandArgs;
		private readonly string _directory;
		
		private Thread _tickThread;
		private ManualResetEvent _resetEvent = new ManualResetEvent(false);
		private TaskQueue _taskQueue;
	  
		public static GameServer Instance{ get; private set; }

		public string Version{ get { return "0.0.1-dev"; } }
		public string[] CommandArgs{ get { return _commandArgs; } }
		public string AppDirectory { get { return _directory; } }
		public bool Running { get; private set; }
		public bool WorldLoaded { get; private set; }
		public CommandExecuter CommandExec{ get; private set; }
		public GameTimer GameTime{ get; private set; }
		public WorldManager WorldMnger { get; private set; }
		public SettingsLoader Settings { get; private set; }

		public GameServer (string[] args)
		{
			_commandArgs = args;
			_directory = Directory.GetCurrentDirectory () + "/";
			Running = true;
			Instance = this;
			_resetEvent = new ManualResetEvent(false);
		}

		public void Run(){

			Settings = new SettingsLoader (_directory + "Settings.ini");
			_taskQueue = new TaskQueue ();
			Logger.Log ("War World Infinity Server Version {0}", Version);
			WorldMnger = new WorldManager (this);
			CommandExec = new CommandExecuter ();
			_tickThread = new Thread (CommandExec.StartCommandLoop);
			_tickThread.Start ();
			GameLoop ();
		}

		public void GameLoop(){
			while (Running) {
				_resetEvent.WaitOne (1);
				_taskQueue.Update();
				if (WorldLoaded){
					if (GameTime.Update()){
						if (GameTime.TickIncrease){
							Logger.Log("Updating world. Tick: {0}", GameTime.Tick.ToString());
						}
					}
				}
				Logger.Update();
			}
		}

		public void StartWorld(World world){
			GameTime = new GameTimer (world.WorldStartTime);
			WorldLoaded = true;
			Logger.Log ("World \"{0}\" started.", world.WorldName);
		}

		public void Exit(){
			Running = false;
			if (WorldLoaded)
				WorldLoaded = false;

		}
	}
}

