using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace debt_fe.Utilities
{
	public class Utility
	{
		/// <summary>
		/// Returns a MD5 hash as a string
		/// </summary>
		/// <param name="input">String to be hashed.</param>
		/// <returns>Hash as string with dashed.</returns>
		public static String ToMD5Hash_WithDash(String input)
		{
			//Check wether data was passed
			if ((input == null) || (input.Length == 0))
			{
				return String.Empty;
			}

			//Calculate MD5 hash. This requires that the string is splitted into a byte[].
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] textToHash = Encoding.Default.GetBytes(input);
			byte[] result = md5.ComputeHash(textToHash);

			//Convert result back to string.
			return System.BitConverter.ToString(result);
		}

		public static string ToMD5Hash(string input)
		{
			// step 1, calculate MD5 hash from input
			MD5 md5 = System.Security.Cryptography.MD5.Create();
			byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
			byte[] hash = md5.ComputeHash(inputBytes);

			// step 2, convert byte array to hex string
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("X2"));
			}
			return sb.ToString();
		}
	}
}