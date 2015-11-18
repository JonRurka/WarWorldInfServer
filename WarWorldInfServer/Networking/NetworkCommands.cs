using System;
using System.Net;
using System.Collections.Generic;
using LibNoise;
using LibNoise.SerializationStructs;
using Newtonsoft.Json;

namespace WarWorldInfServer.Networking {
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
            try {
                Login loginData = JsonConvert.DeserializeObject<Login>(args);
                ResponseType responseType = ResponseType.Failed;
                User.PermissionLevel permission = User.PermissionLevel.None;
                string sessionKey = string.Empty;
                string message = string.Empty;
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
                    }
                    else {
                        message = "User not found!";
                    }
                }
                else {
                    message = "No world instance loaded!";
                    //Logger.LogWarning("User {0} tried to log in with no world loaded!", loginData.name);
                }

                LoginResponse response = new LoginResponse(responseType, permission.ToString(), sessionKey, GameServer.Instance.GameTime.Tick, message);
                return new Traffic("loginresponse", JsonConvert.SerializeObject(response));
            }
            catch (Exception e) {
                Logger.LogError(e.StackTrace);
            }
            return default(Traffic);
        }

        private Traffic GetTerrain_CMD(string args) {
            ResponseType responseType = ResponseType.Failed;
            int seed = 0;
            int width = 0;
            int height = 0;
            IModule module = null;
            List<GradientPresets.GradientKeyData> gradient = new List<GradientPresets.GradientKeyData>();
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

                    MapData data = new MapData(responseType, seed, width, height, gradient, TextureFiles, message);
                    string sendStr = JsonConvert.SerializeObject(data);
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

                }
                else {
                    message = "Invalid session key";
                    MapData data = new MapData(responseType, seed, width, height, gradient, TextureFiles, message);
                    return new Traffic("setterraindata", JsonConvert.SerializeObject(data));
                }
            }
            else {
                message = "World not loaded";
                MapData data = new MapData(responseType, seed, width, height, gradient, TextureFiles, message);
                return new Traffic("setterraindata", JsonConvert.SerializeObject(data));
            }
            return default(Traffic);
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
                        Structures.Structure[] ownedOps = user.GetOwnedOps(getStructures.onlyChanged);
                        Structures.Structure[] visibleOps = user.GetVisibleOps(getStructures.onlyChanged);
                        structures.AddRange(ownedOps);
                        structures.AddRange(visibleOps);
                    }
                    if (getStructures.requestType == GetStructures.RequestType.Owned) {
                        Structures.Structure[] ownedOps = user.GetOwnedOps(getStructures.onlyChanged);
                        structures.AddRange(ownedOps);
                    }
                    if (getStructures.requestType == GetStructures.RequestType.Visible) {
                        Structures.Structure[] visibleOps = user.GetVisibleOps(getStructures.onlyChanged);
                        structures.AddRange(visibleOps);
                    }
                    List<Structure> netStructures = new List<Structure>();
                    for (int i = 0; i < structures.Count; i++) {
                        netStructures.Add(new Structure(
                            structures[i].Location, structures[i].Type.ToString(), structures[i].Owner, "", user.GetStandings(structures[i].Owner).ToString()));
                    }
                    //Logger.Log("Sending {0} structures to {1}", netStructures.Count, user.Name);
                    return new Traffic("setstructures", JsonConvert.SerializeObject(netStructures.ToArray()));
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
                        user.CreateStructure(structureInfo.location, strType);
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
    }
}

