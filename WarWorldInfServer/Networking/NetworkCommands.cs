using System;
using System.Net;
using System.Collections.Generic;
using WarWorldInfinity.LibNoise;
using WarWorldInfinity.Shared;
using Newtonsoft.Json;

namespace WarWorldInfinity.Networking {
    public class NetworkCommands {
        GameServer _server;

        public NetworkCommands() {
            _server = GameServer.Instance;
            _server.SockServ.AddCommand("echo", Echo_CMD);
            _server.SockServ.AddCommand("getsalt", GetSalt_CMD);
            _server.SockServ.AddCommand("login", Login_CMD);
            _server.SockServ.AddCommand("getterrain", GetTerrain_CMD);
            _server.SockServ.AddCommand("getstructures", GetStructures_CMD);
            _server.SockServ.AddCommand("createstructure", CreateStructure_CMD);
            _server.SockServ.AddCommand("changestructure", ChangeStructure_CMD);
            _server.SockServ.AddCommand("structurecommand", StructureCommand_CMD);
            _server.SockServ.AddCommand("inchat", Chat_CMD);
        }

        private Traffic Echo_CMD(string args) {
            Logger.Log("echo: " + args);
            return new Traffic("echo", args);
        }

        private Traffic GetSalt_CMD(string args) {
            string salt = _server.DB.GetSalt(args);
            return new Traffic("completelogin", salt);
        }

        private Traffic Login_CMD(string args) {
            MessageTypes type = MessageTypes.None;
            string message = string.Empty;
            try {
                Login loginData = JsonConvert.DeserializeObject<Login>(args);
                ResponseType responseType = ResponseType.Failed;
                User.PermissionLevel permission = User.PermissionLevel.None;
                string sessionKey = string.Empty;
                if (_server.WorldLoaded) {
                    if (_server.DB.UserExists(loginData.name)) {
                        User user;
                        if (_server.Users.UserExists(loginData.name)) {
                            user = _server.Users.GetUser(loginData.name);
                        }
                        else {
                            user = _server.Users.CreateUser(loginData.name);
                        }
                        bool loggedIn = user.Login(HashHelper.HashPasswordServer(loginData.password, loginData.salt));
                        responseType = loggedIn ? ResponseType.Successfull : ResponseType.Failed;
                        permission = loggedIn ? user.Permission : User.PermissionLevel.None;
                        sessionKey = user.SessionKey;
                        message = user.LoginMessage;
                        Logger.Log("User {0} logged in.", loginData.name);
                        LoginResponse response = new LoginResponse(responseType, permission.ToString(), sessionKey, GameServer.Instance.GameTime.Tick, message);
                        return new Traffic("loginresponse", JsonConvert.SerializeObject(response));
                    }
                    else {
                        message = "User not found!";
                        type = MessageTypes.User_Not_Found;
                    }
                }
                else {
                    message = "No world instance loaded!";
                    type = MessageTypes.World_Not_Loaded;
                    //Logger.LogWarning("User {0} tried to log in with no world loaded!", loginData.name);
                }
            }
            catch (Exception e) {
                Logger.LogError(e.StackTrace);
            }
            return new Traffic("message", JsonConvert.SerializeObject(new Message("_server_", type, message)));
        }

        private Traffic GetTerrain_CMD(string args) {
            ResponseType responseType = ResponseType.Failed;
            int seed = 0;
            int width = 0;
            int height = 0;
            IModule module = null;
            List<GradientPresets.GradientKeyData> gradient = new List<GradientPresets.GradientKeyData>();
            MapData mdata;
            MessageTypes type = MessageTypes.None;
            string[] TextureFiles = new string[0];
            string message = string.Empty;

            if (_server.WorldLoaded) {
                if (_server.Users.SessionKeyExists(args)) {
                    string user = _server.Users.GetConnectedUser(args).Name;
                    TerrainBuilder builder = _server.Worlds.CurrentWorld.Terrain;
                    responseType = ResponseType.Successfull;
                    seed = builder.Seed;
                    width = builder.Width;
                    height = builder.Height;
                    module = builder.NoiseModule;
                    gradient = new List<GradientPresets.GradientKeyData>(builder.GradientPreset);
                    TextureFiles = new List<string>(GradientCreator.TextureFiles.Keys).ToArray();
                    message = "success";

                    // make sure images are cleared. They will be sent seperatly.
                    for (int i = 0; i < gradient.Count; i++) {
                        gradient[i].images.Clear();
                    }

                    mdata = new MapData(responseType, seed, width, height, gradient, TextureFiles, message);
                    string sendStr = JsonConvert.SerializeObject(mdata);
                    string moduleStr = JsonConvert.SerializeObject(module, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                    _server.SockServ.Send(user, "setterrainmodule", moduleStr);
                    _server.SockServ.Send(user, "setterraindata", sendStr);


                    System.Threading.ManualResetEvent reset = new System.Threading.ManualResetEvent(false);
                    foreach (string imageName in GradientCreator.TextureFiles.Keys) {
                        //reset.WaitOne(1);
                        Color[] image = ColorConvert.LibColList(GradientCreator.TextureFiles[imageName]);
                        ImageFileData imageStruct = new ImageFileData(imageName, image);
                        string imageStr = JsonConvert.SerializeObject(imageStruct);
                        _server.SockServ.Send(user, "setimage", imageStr);
                        //Logger.Log("sent image: {0}", imageName);
                    }
                    message = "success";
                    type = MessageTypes.Success;
                    return new Traffic("message", JsonConvert.SerializeObject(new Message("_server_", type, message)));
                }
                else {
                    message = "Invalid session key";
                    type = MessageTypes.Not_Logged_in;
                    return new Traffic("message", JsonConvert.SerializeObject(new Message("_server_", type, message)));
                }
            }
            message = "World not loaded";
            type = MessageTypes.World_Not_Loaded;
            return new Traffic("message", JsonConvert.SerializeObject(new Message("_server_", type, message)));
        }

        private Traffic GetStructures_CMD(string args) {
            GetStructures getStructures = JsonConvert.DeserializeObject<GetStructures>(args);
            MessageTypes type = MessageTypes.None;
            string message = "";
            if (_server.WorldLoaded) {
                if (_server.Users.SessionKeyExists(getStructures.sessionKey)) {
                    User user = _server.Users.GetConnectedUser(getStructures.sessionKey);
                    List<Structures.Structure> structures = new List<Structures.Structure>();
                    if (getStructures.requestType == GetStructures.RequestType.All) {
                        structures.AddRange(user.GetOwnedOps(getStructures.onlyChanged));
                        structures.AddRange(user.GetVisibleOps(getStructures.onlyChanged));
                    }
                    if (getStructures.requestType == GetStructures.RequestType.Owned) {
                        structures.AddRange(user.GetOwnedOps(getStructures.onlyChanged));
                    }
                    if (getStructures.requestType == GetStructures.RequestType.Visible) {
                        structures.AddRange(user.GetVisibleOps(getStructures.onlyChanged));
                    }

                    List<Structure> netStructures = new List<Structure>();
                    for (int i = 0; i < structures.Count; i++) {
                        netStructures.Add(new Structure(
                            structures[i].Location, structures[i].Type.ToString(), structures[i].Owner.Name, "", user.GetStandings(structures[i].Owner).ToString(), structures[i].extraData));
                    }

                    _server.SockServ.Send(user.Name, "setstructurecommands", JsonConvert.SerializeObject(GameServer.Instance.Structures.GetCommands()));
                    return new Traffic("setstructures", JsonConvert.SerializeObject(netStructures.ToArray(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All }));
                }
                else
                    type = MessageTypes.Not_Logged_in;
            }
            else
                type = MessageTypes.World_Not_Loaded;
            return new Traffic("message", JsonConvert.SerializeObject(new Message("_server_", type, message)));
        }

        private Traffic CreateStructure_CMD(string args) {
            SetStructure structureInfo = JsonConvert.DeserializeObject<SetStructure>(args);
            MessageTypes type = MessageTypes.None;
            string message = "";
            if (_server.WorldLoaded) {
                if (_server.Users.SessionKeyExists(structureInfo.sessionKey)) {
                    User user = _server.Users.GetConnectedUser(structureInfo.sessionKey);

                    if (user.CanCreateStructure(structureInfo.location, out type)) {
                        Structures.Structure.StructureType strType = (Structures.Structure.StructureType)Enum.Parse(
                                                                      typeof(Structures.Structure.StructureType),
                                                                      structureInfo.type, true);
                        user.CreateStructure(structureInfo.location, strType, false);
                        //Logger.Log("{0} created a structure.", user.Name);
                        return new Traffic("opcreatesuccess", "success");
                    }
                }
                else
                    type = MessageTypes.Not_Logged_in;
            }
            else
                type = MessageTypes.World_Not_Loaded;
            return new Traffic("message", JsonConvert.SerializeObject(new Message("_server_", type, message)));
        }

        private Traffic ChangeStructure_CMD(string args) {
            SetStructure structureInfo = JsonConvert.DeserializeObject<SetStructure>(args);
            MessageTypes type = MessageTypes.None;
            string message = "";
            if (_server.WorldLoaded) {
                if (_server.Users.SessionKeyExists(structureInfo.sessionKey)) {
                    User user = _server.Users.GetConnectedUser(structureInfo.sessionKey);
                    Structures.Structure.StructureType strType = (Structures.Structure.StructureType)Enum.Parse(
                                                                 typeof(Structures.Structure.StructureType), structureInfo.type);
                    if (user.CanUpgradeStructure(structureInfo.location, strType, out type)) {
                        user.changeStructure(structureInfo.location, strType);
                        return new Traffic("opcreatesuccess", "success");
                    }
                }
                else
                    type = MessageTypes.Not_Logged_in;
            }
            else
                type = MessageTypes.World_Not_Loaded;
            return new Traffic("message", JsonConvert.SerializeObject(new Message("_server_", type, message)));
        }

        private Traffic StructureCommand_CMD(string args) {
            StructureCommand command = JsonConvert.DeserializeObject<StructureCommand>(args);
            MessageTypes type = MessageTypes.None;
            string message = "";

            if (_server.WorldLoaded) {
                if (_server.Users.SessionKeyExists(command.sessionKey)) {
                    User user = _server.Users.GetConnectedUser(command.sessionKey);
                    user.SendCommand(command.location, command.command);
                    type = MessageTypes.Success;
                }
                else
                    type = MessageTypes.Not_Logged_in;
            }
            else
                type = MessageTypes.World_Not_Loaded;

            return new Traffic("message", JsonConvert.SerializeObject(new Message("_server_", type, message)));
        }

        private Traffic Chat_CMD(string args) {
            ChatMessage msg = JsonConvert.DeserializeObject<ChatMessage>(args);
            MessageTypes type = MessageTypes.None;
            string message = "";

            if (_server.WorldLoaded) {
                if (_server.Users.SessionKeyExists(msg.sessionKey)) {
                    _server.Chat.Submit(msg.player, msg.message);
                    type = MessageTypes.Success;
                }
                else
                    type = MessageTypes.Not_Logged_in;
            }
            else
                type = MessageTypes.World_Not_Loaded;
            return new Traffic("message", JsonConvert.SerializeObject(new Message("_server_", type, message)));
        }
    }
}

