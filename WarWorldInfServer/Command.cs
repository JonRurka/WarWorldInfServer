using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarWorldInfinity
{
    public class Command : Attribute
    {
        public string command;
        public string command_args;
        public string description_small;
        public string description_Long;
        public User.PermissionLevel permission;
        public CommandExecuter.CommandFunction callback;

        public Command(string command, string commandArgs, string descSmall, string descLarge, User.PermissionLevel permission, CommandExecuter.CommandFunction callback) {
            this.command = command;
            this.command_args = commandArgs;
            this.description_small = descSmall;
            this.description_Long = descLarge;
            this.permission = permission;
            this.callback = callback;
        }

        public Command(string command, string commandArgs, string descSmall, string descLarge, User.PermissionLevel permission, string callback)
        {
            this.command = command;
            this.command_args = commandArgs;
            this.description_small = descSmall;
            this.description_Long = descLarge;
            this.permission = permission;
            this.callback = CommandDescription.GetCallbackFromString(command, callback);
        }
    }
}
