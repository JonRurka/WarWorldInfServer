using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace WarWorldInfServer
{
	public static class SessionKeyGenerator
	{
		private static List<string> _generatedKeys = new List<string> ();

		public static string RandomKey(int length){
			char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
			char[] identifier = new char[length];
			byte[] randomData = new byte[length];
			using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider()) {
				rng.GetBytes(randomData);
			}
			for (int i = 0; i < identifier.Length; i++) {
				int pos = randomData[i] % chars.Length;
				identifier[i] = chars[pos];
			}
			string key = new string (identifier);
			if (_generatedKeys.Contains (key))
				key = RandomKey (length);
			return key;
		}
	}
}

