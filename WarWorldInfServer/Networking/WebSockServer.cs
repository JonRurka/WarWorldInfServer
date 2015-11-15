using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using LibNoise.SerializationStructs;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;

namespace WarWorldInfServer.Networking {
    public class WebSockServer {
        /*private TcpListener _server;
        private int _serverPort;

        public WebSockServer(int serverPort) {
            _serverPort = serverPort;
            TaskQueue.QeueAsync("WebSocketServer", Run);
        }

        public void Run() {
            _server = new TcpListener(IPAddress.Parse("127.0.0.1"), _serverPort);
            _server.Start();
            Logger.Log("Server listening...");
            TcpClient client = _server.AcceptTcpClient();
            Logger.Log("A client connected.");
            NetworkStream stream = client.GetStream();
            while (true) {
                while (!stream.DataAvailable);

                byte[] bytes = new byte[client.Available];
                stream.Read(bytes, 0, bytes.Length);

                string data = Encoding.UTF8.GetString(bytes);

                if (new Regex("^GET").IsMatch(data)) {
                    Logger.Log("handshake received.");
                    //byte[] response = Encoding.UTF8.GetBytes
                    byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                                                            + "Connection: Upgrade" + Environment.NewLine
                                                            + "Upgrade: websocket" + Environment.NewLine
                                                            + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                                                                SHA1.Create().ComputeHash(
                                                                    Encoding.UTF8.GetBytes(
                                                                        new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                                                                        )
                                                                    )
                                                                ) + Environment.NewLine
                                                            );
                    stream.Write(response, 0, response.Length);
                }
                else
                {
                    string str = string.Empty;
                    for (int i = 0; i <bytes.Length;i++) {
                        str += bytes[i].ToString();
                    }
                    Logger.Log(str);
                }
            }
        }*/

        public delegate Traffic CMD(string data);

        public class UserConnect : WebSocketBehavior {
            private WebSockServer _server;
            public UserConnect(WebSockServer server) {
                _server = server;
                _server.UsrConnect = this;
            }

            protected override void OnMessage(MessageEventArgs e) {
                string[] input = Encoding.UTF8.GetString(e.RawData).Split('&');
                if (GameServer.Instance.DB.UserExists(input[0])) {
                    if (!_server.UserExists(input[0])) {
                        string domain = HashHelper.RandomKey(8);
                        //_server.AddUser(input[0], domain);
                        Logger.Log("{0} connected", input);
                        // TODO: new encryption passkey for each client.
                        Send("domain=" + domain + "&key=N/A");
                    }
                    else {
                        Logger.Log("already connected: {0}", input);
                        Send("user already connected.");
                    }
                }
                else {
                    Logger.Log("user not found: {0}", input);
                    Send("user not found");
                }
            }
        }

        public class WebSockUser : WebSocketBehavior {
            public string user { get; private set; }
            private WebSockServer _server;
            private string _encryptionPass = "QH5SnB7eXckcAqa8yUGPbqEsQ1XL9eo";
            public bool Connected { get; set; }
            public bool LoggedIn { get; set; }

            public WebSockUser(WebSockServer server) {
                _server = server;
            }

            protected override void OnMessage(MessageEventArgs e) {
                string input = HashHelper.Decrypt(Convert.ToBase64String(e.RawData), _encryptionPass);
                if (Connected) {
                    AddConnection(input);
                    Traffic result = _server.ProcessData(input);
                    if (!result.command.IsNullOrEmpty()) {
                        SendString(JsonConvert.SerializeObject(result));
                    }
                }
            }

            protected override void OnOpen() {
                Connected = true;
            }

            protected override void OnClose(CloseEventArgs e) {
                Connected = false;
            }

            public void SendString(string data) {
                if (Connected) {
                    string encryptedData = HashHelper.Encrypt(data, _encryptionPass);
                    Send(encryptedData);
                }
            }

            private void AddConnection(string input) {
                Traffic result = JsonConvert.DeserializeObject<Traffic>(input);
                if (result.command == "login") {
                    Login loginData = JsonConvert.DeserializeObject<Login>(result.data);
                    _server.AddUser(loginData.name, this);
                    Logger.Log(loginData.name + " connected.");
                }
            }
        }

        public class Echo : WebSocketBehavior {
            protected override void OnMessage(MessageEventArgs e) {
                string input = Encoding.UTF8.GetString(e.RawData);
                Logger.Log(input);
                Send(input);
            }
        }

        public int Port { get; private set; }
        public int DownBps { get; private set; }
        public int UpBps { get; private set; }
        public int ReceivedPs { get; private set; }
        public int SentPs { get; private set; }

        private WebSocketServer _server;
        private UserConnect UsrConnect { get; set; }
        private Dictionary<string, WebSockUser> ConnectedUsers { get; set; }
        private Dictionary<string, CMD> Commands { get; set; }
        private Stopwatch _watch;
        private int _receivedBytes;
        private int _sentBytes;
        private int _received;
        private int _sent;

        public WebSockServer(int port) {
            Port = port;
            Commands = new Dictionary<string, CMD>();
            ConnectedUsers = new Dictionary<string, WebSockUser>();
            _server = new WebSocketServer(port);
            _server.AddWebSocketService("/echo", () => new Echo() 
                { IgnoreExtensions = true });
            _server.AddWebSocketService("/server", () => new WebSockUser(this) 
                { IgnoreExtensions = true });
            _server.Start();
            _watch = new Stopwatch();
            _watch.Start();
        }

        public void Update() {
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
            }
        }

        public void Stop() {
            Stop("Server stopped.");
        }

        public void Stop(string reason) {
            Stop(CloseStatusCode.Normal, reason);
        }

        public void Stop(CloseStatusCode code, string reason) {
            if (_server != null)
                _server.Stop(code, reason);
        }

        public void AddUser(string user, WebSockUser instance) {
            if (!UserExists(user)) {
                ConnectedUsers.Add(user, instance);
            }
        }

        public void RemoveUser(string user, string reason) {
            if (UserExists(user)) {
                Send(user, "close", reason);
                ConnectedUsers[user].Connected = false;
                ConnectedUsers.Remove(user);
            }
        }

        public bool UserExists(string user) {
            return ConnectedUsers.ContainsKey(user);
        }

        public void AddCommand(string cmd, CMD callback) {
            if (!CommandExists(cmd.ToLower())) {
                Commands.Add(cmd.ToLower(), callback);
            }
        }

        public void RemoveCommand(string cmd) {
            if (CommandExists(cmd.ToLower()))
                Commands.Remove(cmd.ToLower());
        }

        public bool CommandExists(string Cmd) {
            return Commands.ContainsKey(Cmd.ToLower());
        }

        public void Broadcast(string command, string data) {
            Send(ConnectedUsers.Keys.ToArray(), new Traffic(command, data));
        }

        public void Broadcast(Traffic traffic) {
            Send(ConnectedUsers.Keys.ToArray(), traffic);
        }

        public void Send(string[] users, string command, string data) {
            for (int i = 0; i < users.Length; i++) {
                Send(users[i], new Traffic(command, data));
            }
        }

        public void Send (string[] users, Traffic traffic) {
            for (int i = 0; i < users.Length; i++) {
                Send(users[i], traffic);
            }
        }

        public void Send(string user, string command, string data) {
            Send(user, new Traffic(command, data));
        }

        public void Send(string user, Traffic traffic) {
            if (UserExists(user)) {
                ConnectedUsers[user].SendString(JsonConvert.SerializeObject(traffic));
            }
        }

        public Traffic ProcessData(string data) {
            _receivedBytes += data.Length * sizeof(char);
            _received++;
            Traffic traffic = JsonConvert.DeserializeObject<Traffic>(data);
            if (CommandExists(traffic.command))
                return Commands[traffic.command.ToLower()](traffic.data);
            return default(Traffic);
        }
    }
}
