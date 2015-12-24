using System;

namespace WarWorldInfinity
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
