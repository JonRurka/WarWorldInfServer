using System;
using System.Text;
using System.Net;

namespace WarWorldInfServer
{
	public class NetServer
	{
		// reference JTechPingClient.


		/* Notify clients about new tick.
		 * Send back information when requrest from the client. 
		 */

		// client logs onto Logon server which gives client a session key
		// When client connects to a server, the session key is set to the server
		// User logs onto server with session key



		public int ServerPort { get; private set;}

		public NetServer (int serverPort)
		{
			ServerPort = serverPort;
		}

		public void SendAll(string data)
		{
			/*foreach (var ip in clients.Keys)
			{
				Send(ip, data);
			}*/
		}
		
		public void Send(string ip, string data)
		{
			byte[] sendBytes = Encoding.ASCII.GetBytes(data);
			Send(ip, sendBytes);
		}

		public void Send(string ip, byte[] data){
			// check if user exists.

			// get client port

			// send
		}
	}
}

