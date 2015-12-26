using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using WarWorldInfinity.LibNoise;
using WarWorldInfinity.Networking;
using WarWorldInfinity.Structures;

namespace WarWorldInfinity
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
        public static char sepChar {  get { return Path.DirectorySeparatorChar; } }
        public static bool isMono { get { return (Type.GetType("Mono.Runtime") != null); } }


        /// <summary>
        /// version format: first.second.third
        /// first is incremented if save is no longer compatible.
        /// second is incremented if save needs to be converted.
        /// third is incremented if save is completely compatible.
        /// 
        /// version must be incremented each release.
        /// </summary>
        public string Version{ get { return "0.0.1-dev"; } }
        /// <summary>
        /// Increment when protocal becomes none-backwards compatible
        /// with client. 
        /// </summary>
        public string NetProtocalVersion { get { return "0"; } }
		public string[] CommandArgs{ get { return _commandArgs; } }
		public string AppDirectory { get { return _directory; } }
		public bool Running { get; private set; }
		public bool WorldLoaded { get; set; }
		public CommandExecuter CommandExec{ get; private set; }
		public GameTimer GameTime{ get; private set; }
		public WorldManager Worlds { get; private set; }
		public AppSettings Settings { get; private set; }
		public UserManager Users { get; private set; }
		public NetServer Net { get; private set; }
		public DataBase DB { get; private set; }
        public AutoSave autoSaver { get; private set; }
        public WebSockServer SockServ { get; private set; }
        public FrameCounter FCounter { get; private set; }
        public StructureControl Structures { get; private set; }
        public ChatProcessor Chat { get; private set; }
        public AllianceManager Alliances { get; private set; }

		public GameServer (string[] args)
		{
			_commandArgs = args;
			_directory = Directory.GetCurrentDirectory () + sepChar;
			GradientPresets.AppDirectory = _directory;
			Running = true;
			Instance = this;
			_resetEvent = new ManualResetEvent(false);
		}

		public void Run(){
            Logger.Log("War World Infinity Server Version {0}", Version);
            Settings = new AppSettings (_directory + "Settings.ini");
			_taskQueue = new TaskQueue ();
			CommandExec = new CommandExecuter ();
            //SocketPolicyServer.LoadAll();
            SockServ = new WebSockServer(AppSettings.ServerPort);
            _netCommands = new NetworkCommands();
			DB = new DataBase (AppSettings.DbServer, AppSettings.Database);
			DB.Connect (AppSettings.DbUsername, AppSettings.DbPassword);
			Worlds = new WorldManager (this);
            Structures = new StructureControl();
            Chat = new ChatProcessor();
            Users = new UserManager ();
            autoSaver = new AutoSave();
            FCounter = new FrameCounter();
            Alliances = new AllianceManager();
            Noise2D.RunAsync += TaskQueue.QeueAsync;
            _tickThread = new Thread (CommandExec.StartCommandLoop);
			_tickThread.Start ();
			GameLoop ();
		}

		public void GameLoop(){
			while (Running) {
                _resetEvent.WaitOne (1);
                FCounter.Update();
                try {
					if (WorldLoaded && GameTime != null){
                        Users.Frame();
						if (GameTime.Update()){ // every second
							if (GameTime.TickIncrease){ // update when enough seconds pass to increase tick.
								Logger.Log("Updating world. Tick: {0}", GameTime.Tick.ToString());
                                Users.TickUpdate();
                                Structures.TickUpdate();
                                _taskQueue.Update();
                                Save();
                                SockServ.Broadcast("tick", GameTime.Tick.ToString());
							}
						}
					}
					//Net.Update();
					_taskQueue.Update(); // run items queued during frame.
					Logger.Update(); // print items sent to log during frame.
                }
				catch (Exception e){
					Logger.LogError("{0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
				}
			}
		}

		public string CreatePlayer(string username, string password, string permission, string email){
            User.PermissionLevel level;
            if (Enum.TryParse(permission, true, out level)) {
                string salt = HashHelper.RandomKey(32);
                DB.AddNewPlayer(username, HashHelper.HashPasswordFull(password, salt), salt, permission, email);
                return string.Format("User {0} created as {1}.", username, permission);
            }
            else
                return "Invalid permission level.";
		}

		public void StartWorld(World world){
			GameTime = new GameTimer (world.WorldStartTime);
            autoSaver.Start(Save, AppSettings.AutoSaveInterval);
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
            SockServ.Stop("stopping server...");
			TaskQueue.Close ();
			Running = false;
            autoSaver.Stop();
            _tickThread.Abort ();
		}

        private bool IsMonoRuntime()
        {
            return (Type.GetType("Mono.Runtime") != null);
        }
	}
}

