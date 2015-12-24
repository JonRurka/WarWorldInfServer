using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarWorldInfinity.LibNoise;
using WarWorldInfinity.Shared;
using Newtonsoft.Json;

namespace WarWorldInfinity {
    public class ChatProcessor {
        public ChatProcessor() {
            
        }

        public void Submit(string player, string message) {
            if (message != string.Empty) {
                Logger.Log("{0}: {1}", player, message);
                if (message.StartsWith("/")) {
                    message = GameServer.Instance.CommandExec.ExecuteCommand(player, message.Replace("/", ""));
                    ChatMessage mes = new ChatMessage("", "Server", message);
                    GameServer.Instance.SockServ.Send(player, "inchat", JsonConvert.SerializeObject(mes));
                }
                else {
                    ChatMessage mes = new ChatMessage("", player, message);
                    GameServer.Instance.SockServ.Broadcast("inchat", JsonConvert.SerializeObject(mes));
                }
            }
        }
    }
}
