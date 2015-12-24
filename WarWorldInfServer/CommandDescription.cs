using System;
using System.Reflection;

namespace WarWorldInfinity
{
	public struct CommandDescription{
		public string command;
		public string command_args;
		public string description_small;
		public string description_Long;
        public User.PermissionLevel permission;
        public CommandExecuter.CommandFunction callback;

		
		public CommandDescription(string _command, string _command_args, string _description_small, string _description_Long, User.PermissionLevel permission, string _callback) {
			command = _command.ToLower();
			command_args = _command_args;
			description_small = _description_small;
			description_Long = _description_Long;
            this.permission = permission;
			callback = GetCallbackFromString(_command, _callback);
		}
		
		public CommandDescription(string _command, string _command_args, string _description_small, string _description_Long, User.PermissionLevel permission, CommandExecuter.CommandFunction _callback) {
			command = _command.ToLower();
			command_args = _command_args;
			description_small = _description_small;
			description_Long = _description_Long;
            this.permission = permission;
            callback = _callback;
		}
		
		public CommandDescription(string _command, string _command_args, string _description_small, User.PermissionLevel permission, string _callback) {
			command = _command.ToLower();
			command_args = _command_args;
			description_small = _description_small;
			description_Long = string.Empty;
            this.permission = permission;
            callback = GetCallbackFromString(_command, _callback);
		}
		
		public CommandDescription(string _command, string _command_args, string _description_small, User.PermissionLevel permission, CommandExecuter.CommandFunction _callback) {
			command = _command.ToLower();
			command_args = _command_args;
			description_small = _description_small;
			description_Long = string.Empty;
            this.permission = permission;
            callback = _callback;
		}
		
		private static CommandExecuter.CommandFunction GetCallbackFromString(string command, string callback){
			CommandExecuter.CommandFunction result = null;
			MethodInfo methodInf = typeof(CommandExecuter).GetMethod (callback, 
			                                                          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			if (methodInf != null) {
				object typeInstance = GameServer.Instance.CommandExec;
				if (methodInf.IsStatic) {
					result = (CommandExecuter.CommandFunction)Delegate.CreateDelegate (typeof(CommandExecuter.CommandFunction), methodInf);
				} else
					result = (CommandExecuter.CommandFunction)Delegate.CreateDelegate (typeof(CommandExecuter.CommandFunction), typeInstance, methodInf);
			} else
				Logger.LogError ("Could not find function {0}.", callback);
			return result;
		}
	}
}

