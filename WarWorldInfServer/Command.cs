using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarWorldInfinity
{
    public class Command : Attribute
    {
        public string command { get; set; }
        public string command_args { get; set; }
        public string description_small { get; set; }
        public string description_Long { get; set; }
        public User.PermissionLevel permission { get; set; }

        public Command(string command, string commandArgs, string descSmall, string descLarge, User.PermissionLevel permission) {
            this.command = command;
            this.command_args = commandArgs;
            this.description_small = descSmall;
            this.description_Long = descLarge;
            this.permission = permission;
        }
    }
}
