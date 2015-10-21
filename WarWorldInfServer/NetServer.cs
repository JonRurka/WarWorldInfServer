using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

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
		private Stopwatch _watch;
		private int _receivedBytes;
		private int _sentBytes;
		private int _received;
		private int _sent;
		private Label _downLabel;
		private Label _upLabel;
		private Label _receivedLabel;
		private Label _sentLabel;


		public int ServerPort { get; private set; }
		public int ClientPort { get; private set; }
		public Dictionary<string, CMD> Commands { get; private set; }
		public int DownBps { get; private set; }
		public int UpBps { get; private set; }
		public int ReceivedPs { get; private set;}
		public int SentPs{ get; private set; }

		public NetServer (int serverPort, int clientPort)
		{
			ServerPort = serverPort;
			ClientPort = clientPort;
			Commands = new Dictionary<string, CMD> ();
			_client = new UdpClient (serverPort);
			_receivedBytes = 0;
			_watch = new Stopwatch ();
			_watch.Start ();
			TaskQueue.QeueAsync("NetThread", ()=> Receive());
			_downLabel = new Label (){Text = "down: " + GetDataRate(DownBps), Location = new Point(0, 0), Size = new Size(200, 20)};
			_upLabel = new Label (){Text = "up: " + GetDataRate(UpBps), Location = new Point(0, 20), Size = new Size(200, 20)};
			_receivedLabel = new Label (){Text = "received: " + ReceivedPs + " /s", Location = new Point(0, 40), Size = new Size(200, 20)};
			_sentLabel = new Label (){Text = "sent: " + SentPs + " /s", Location = new Point(0, 60), Size = new Size(200, 20)};
		}

		public void Update(){
			long milliseconds = _watch.ElapsedMilliseconds;
			if (milliseconds % 1000 == 0) {
				DownBps = _receivedBytes;
				_receivedBytes = 0;

				UpBps = _sentBytes;
				_sentBytes = 0;

				ReceivedPs = _received;
				_received = 0;

				SentPs = _sent;
				_sent = 0;

				if (_downLabel != null && _upLabel != null) {
					_downLabel.Text = "down: " + GetDataRate(DownBps);
					_upLabel.Text = "up: " + GetDataRate(UpBps);
					_receivedLabel.Text = "received: " + ReceivedPs + " /s";
					_sentLabel.Text = "sent: " + SentPs + " /s";
				}
			}
		}

		public string GetDataRate(float bytes){
			float kBytes = bytes / 1024f;
			float mBytes = kBytes / 1024f;
			float gBytes = mBytes / 1024f;

			if (bytes < 1024)
				return bytes + " B/s";
			else if (kBytes < 1024)
				return kBytes.ToString("0.000") + " KB/s";
			else if (mBytes < 1024)
				return mBytes.ToString("0.000") + " MB/s";
			else
				return gBytes.ToString("0.000") + " GB/s";
		}

		public void DisplayDataRate(){
			TaskQueue.QeueAsync ("rateForm", () => {
				Form form = new Form ();
				Rectangle rect = Screen.FromControl(form).Bounds;
				//Logger.Log("{0}, {1}, {2}, {3}", rect.Left, rect.Top, rect.Width, rect.Height);
				form.StartPosition = FormStartPosition.CenterScreen;
				//form.Location = new Point(rect.Left, rect.Top);
				form.Width = 200;
				form.Height = 100;
				form.FormBorderStyle = FormBorderStyle.FixedDialog;
				form.Controls.Add(_downLabel);
				form.Controls.Add(_upLabel);
				form.Controls.Add(_receivedLabel);
				form.Controls.Add(_sentLabel);
				form.ShowDialog();
				form.Close();
				form.Dispose();
			});
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
				_sentBytes += data.Length;
				_sent ++;
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

		public void Close(){
			_run = false;
			_client.Close();
			Logger.Log("Net controller closed.");
		}

		private void Receive(){
			_run = true;
			while (_run) {
				try {
					IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, ClientPort);
					byte[] receivedData = _client.Receive(ref endPoint);
					TaskQueue.QueueMain(()=>ProcessData(endPoint, receivedData));
				}
				catch (SocketException e){

				}
				catch (Exception e){
					Console.WriteLine("{0}\n{1}", e.Message, e.StackTrace);
				}
			}
			Console.WriteLine("closed");
		}

		private void ProcessData(IPEndPoint endpoint, byte[] data){
			_receivedBytes += data.Length;
			_received++;
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

