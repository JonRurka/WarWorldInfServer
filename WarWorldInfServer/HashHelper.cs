using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace WarWorldInfServer
{
	public static class HashHelper
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

		public static string MD5Hash(string value){
			StringBuilder sBuilder = new StringBuilder();
			MD5 md5Hash = MD5.Create (); 
				byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(value));
			for (int i = 0; i < data.Length; i++){
				sBuilder.Append(data[i].ToString("x2"));
			}
			md5Hash.Dispose();
			return sBuilder.ToString ();
		}

		public static string SHA256Hash(string value){
			StringBuilder sBuilder = new StringBuilder();
			SHA256 sha256 = SHA256Managed.Create ();
			byte[] data = sha256.ComputeHash (Encoding.UTF8.GetBytes (value));
			for (int i = 0; i < data.Length; i++){
				sBuilder.Append(data[i].ToString("x2"));
			}
			sha256.Dispose();
			return sBuilder.ToString ();
		}

		public static string PBKDF2(string value, int iterations){
			return "";
		}

		public static string HashPasswordFull(string value, string salt){
			return HashPasswordServer( HashPasswordClient (value, salt), salt);
		}

		public static string HashPasswordClient(string value, string salt){
			return MD5Hash(value + salt);
		}

		public static string HashPasswordServer(string value, string salt){
			return SHA256Hash(salt + value); 
		}
	}
}

