using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WarWorldInfServer {
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

        public class UserConnect : WebSocketBehavior {
            private WebSockServer _server;
            public UserConnect(WebSockServer server) {
                _server = server;
                _server.UsrConnect = this;
            }

            protected override void OnMessage(MessageEventArgs e) {
                string input = Encoding.UTF8.GetString(e.RawData);
                //TODO: check if user exits.
                _server.AddUser(input);
                Logger.Log("{0} connected", input);
                Send("login success");
            }
        }

        public class WebSockUser : WebSocketBehavior {
            private string _user;
            private WebSockServer _server;

            public WebSockUser(WebSockServer server, string user) {
                _user = user;
                _server = server;
                server.ConnectedUsers.Add(user, this);
            }

            protected override void OnMessage(MessageEventArgs e) {
                string input = Encoding.UTF8.GetString(e.RawData);
                Logger.Log("received: " + input);
                Send(input);
            }

            protected override void OnOpen() {
                
            }
        }

        private WebSocketServer _server;
        private int _port;
        public Dictionary<string, WebSockUser> ConnectedUsers { get; private set; }
        public UserConnect UsrConnect { get; private set; }
        private Dictionary<string, WebSocketServer> _servers;

        public WebSockServer(int port) {
            _port = port;
            ConnectedUsers = new Dictionary<string, WebSockUser>();
            _servers = new Dictionary<string, WebSocketServer>();
            _server = new WebSocketServer(port);
            _server.AddWebSocketService<UserConnect>("/connect", () => new UserConnect(this));
            _server.Start();
        }
        
        public void Stop() {
            Stop("Web Socket Server stopped.");
        }

        public void Stop(string reason) {
            Stop(CloseStatusCode.Normal, reason);
        }

        public void Stop(CloseStatusCode code, string reason) {
            if (_server != null)
                _server.Stop(code, reason);
        }

        public void AddUser(string user) {
            _server.AddWebSocketService<WebSockUser>("/" + user, () => new WebSockUser(this, user));
        }
    }
}
