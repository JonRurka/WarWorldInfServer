using System;
using System.IO;
using System.Threading;
using System.Net;
using Newtonsoft.Json;

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
		public UserManager Users { get; private set; }
		public NetServer Net { get; private set; }

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
			CommandExec = new CommandExecuter ();
			Net = new NetServer (Settings.ServerPort, Settings.ClientPort);
			AddNetworkCommands ();
			WorldMnger = new WorldManager (this);
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
			Users = new UserManager ();
			WorldLoaded = true;
			Logger.Log ("World \"{0}\" started.", world.WorldName);
		}

		public void Exit(){
			Running = false;
			if (WorldLoaded)
				WorldLoaded = false;
		}

		private void AddNetworkCommands(){
			Net.AddCommand ("ping", Ping_CMD);
			Net.AddCommand ("login", Login_CMD);
		}

		private void Ping_CMD(IPEndPoint endPoint, string args){
			//Logger.Log ("Received ping from " + endPoint.Address.ToString ());
			Net.Send (endPoint.Address.ToString (), Net.ClientPort, "pingsuccess#");
		}

		private void Login_CMD(IPEndPoint endPoint, string args){
			SerializationStructs.Login loginData = JsonConvert.DeserializeObject<SerializationStructs.Login> (args);
			SerializationStructs.LoginResponse.ResponseType responseType = SerializationStructs.LoginResponse.ResponseType.Failed;
			User.PermissionLevel permission = User.PermissionLevel.None;
			string sessionKey = string.Empty;
			string message = string.Empty;
			if (WorldLoaded) {
				if (Users.UserExists(loginData.name)){
					User user = Users.GetUser (loginData.name);
					bool loggedIn = user.Login (endPoint.Address.ToString (), loginData.password);
					responseType = loggedIn ? SerializationStructs.LoginResponse.ResponseType.Successfull : SerializationStructs.LoginResponse.ResponseType.Failed;
					permission = loggedIn ? user.Permission : User.PermissionLevel.None;
					sessionKey = user.SessionKey;
					message = user.LoginMessage;
					Logger.Log("User {0} logged in from {1}", loginData.name, endPoint.Address.ToString());
				}
				else{
					message = "User not found!";
				}
			} else {
				message = "No world instance loaded!";
				//Logger.LogWarning("User {0} tried to log in with no world loaded!", loginData.name);
			}

			SerializationStructs.LoginResponse response = new SerializationStructs.LoginResponse (responseType, permission, sessionKey, message);
			Net.Send(endPoint.Address.ToString(), Net.ClientPort, "loginresponse#" + JsonConvert.SerializeObject(response));
		}
	}
}

