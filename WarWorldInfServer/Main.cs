using System;
using System.Drawing;
using System.IO;
using LibNoise;
using LibNoise.Models;
using LibNoise.Modifiers;

namespace WarWorldInfServer
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			GameServer server = new GameServer (args);
			server.Run();
		}
	}
}
