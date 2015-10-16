using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WarWorldInfServer
{
	public class NetServer
	{
		// reference JTechPingClient.


		/* Notify clients about new tick.
		 * Send back information when requrest from the client. 
		 */

		// client logs onto Logon server which gives client a session key
		// When client connects to a server, the session key is set to the server
		// User logs onto server with session key

		/* User sends command to this with camera location and zoom level to query for data
		 * for that location.
		 */ 

		public delegate void CMD(IPEndPoint endPoint, string data);

		private UdpClient _client;
		private bool _run;

		public int ServerPort { get; private set; }
		public int ClientPort { get; private set; }
		public Dictionary<string, CMD> Commands { get; private set; }

		public NetServer (int serverPort, int clientPort)
		{
			ServerPort = serverPort;
			ClientPort = clientPort;
			Commands = new Dictionary<string, CMD> ();
			_client = new UdpClient (serverPort);
			Thread thread = new Thread (Receive);
			thread.Start ();
		}

		public void SendAll(string data)
		{
			User[] users = GameServer.Instance.Users.GetUsers ();
			for (int i = 0; i < users.Length; i++) {
				Send(users[i], data);
			}
		}
		
		public void Send(User user, string data)
		{
			byte[] sendBytes = Encoding.ASCII.GetBytes(data);
			Send(user.Ip, ClientPort, data);
		}

		public void Send(string ip, int port, string data){
			Send (ip, port, Encoding.ASCII.GetBytes (data + "$"));
		}

		public void Send(string ip, int port, byte[] data){
			if (_client != null) {
				IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
				_client.Send(data, data.Length, endPoint);
			}
		}

		public void AddCommand(string cmd, CMD callback){
			if (!CommandExists (cmd.ToLower())) {
				Commands.Add(cmd.ToLower(), callback);
			}
		}

		public void RemoveCommand(string cmd){
			if (CommandExists (cmd.ToLower()))
				Commands.Remove (cmd.ToLower());
		}

		public bool CommandExists(string Cmd){
			return Commands.ContainsKey (Cmd.ToLower ());
		}

		private void Receive(){
			_run = true;
			while (_run) {
				IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, ClientPort);
				byte[] receivedData = _client.Receive(ref endPoint);
				TaskQueue.QueueMain(()=>ProcessData(endPoint, receivedData));
			}
		}

		private void ProcessData(IPEndPoint endpoint, byte[] data){
			string inString = Encoding.ASCII.GetString (data);
			if (inString.Contains ("#") && inString.Contains ("$")) {
				string[] parts = inString.Substring(0, inString.IndexOf('$')).Split('#');
				if (parts.Length == 2){
					string cmd = parts[0];
					string args = parts[1];
					if (CommandExists (cmd.ToLower())){
						Commands[cmd.ToLower()](endpoint, args);
					}
					else
						Logger.LogError("Received unknown command \"{0}\" from {1}", cmd, endpoint.Address.ToString());
				}
			}
		}
	}
}

