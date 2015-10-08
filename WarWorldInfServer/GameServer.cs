using System;
using System.IO;
using System.Threading;

namespace WarWorldInfServer
{
	public class GameServer
	{
		private readonly string[] _commandArgs;
		private readonly string _directory;
		private static GameServer _instance;


		private Thread _tickThread;
		private bool _running;
		private bool _worldLoaded;
		private CommandExecuter _commandExec;
		private GameTimer _gameTimer;
		private WorldManager _worlds;
		private ManualResetEvent resetEvent = new ManualResetEvent(false);

		public static string Version{ get { return "0.0.1-dev"; } }
		public static GameServer Instance{ get { return _instance; } }

		public string[] CommandArgs{ get { return _commandArgs; } }
		public string AppDirectory { get { return _directory; } }
		public bool Running { get { return _running; } }
		public bool WorldLoaded { get { return _worldLoaded; } }
		public CommandExecuter CommandExec{ get { return _commandExec; } }
		public GameTimer GameTime{ get { return _gameTimer; } }
		public WorldManager WorldMnger { get { return _worlds; } }

		public GameServer (string[] args)
		{
			_commandArgs = args;
			_directory = Directory.GetCurrentDirectory () + "/";
			_running = true;
			_instance = this;
		}

		public void Run(){
			Logger.Log ("War World Infinity Server Version 0.0.1-dev");
			resetEvent = new ManualResetEvent(false);
			_worlds = new WorldManager (this);
			_tickThread = new Thread (GameLoop);
			_tickThread.Start ();
			_commandExec = new CommandExecuter ();
			_commandExec.StartCommandLoop ();
		}

		public void GameLoop(){
			while (_running) {
				resetEvent.WaitOne (1);
				Logger.Update();
				if (_worldLoaded){
					if (_gameTimer.Update()){
						if (_gameTimer.TickIncrease){
							Logger.Log("Updating world. Tick: {0}", _gameTimer.Tick.ToString());
						}
					}
				}
			}
		}

		public void StartWorld(World world){
			_gameTimer = new GameTimer (world.WorldStartTime);
			_worldLoaded = true;
			Logger.Log ("World \"{0}\" started.", world.WorldName);
		}

		public void Exit(){
			_running = false;
			if (_worldLoaded)
				_worldLoaded = false;

		}
	}
}

