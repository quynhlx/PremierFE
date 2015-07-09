using System;
using System.Collections.Generic;
using System.IO;
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

		/// <summary>
		/// create md5 string 
		/// </summary>
		/// <param name="input">a string to md5</param>
		/// <returns>a md5 string</returns>
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

		/// <summary>
		/// format file name with random string
		/// </summary>
		/// <param name="filename">a string of filename like my-file.ext</param>
		/// <returns>a string kind of my-file-random-string.ext</returns>
		public static string FormatFileName(string filename)
		{
			if (string.IsNullOrEmpty(filename))
			{
				return "";
			}

			var ext = string.Empty;
			var name = GetFileName(filename, out ext);
			var randomString = Path.GetRandomFileName().Replace(".", string.Empty);

			if (string.IsNullOrEmpty(name))
			{
				return "";
			}

			var formatted = string.Format("{0}-{1}.{2}",name,randomString,ext);

			return formatted;
		}

		/// <summary>
		/// get name without extension of file
		/// </summary>
		/// <param name="filename">a string of filename with extension</param>
		/// <param name="ext">an out string of file extension</param>
		/// <returns>a string of file name without extension</returns>
		public static string GetFileName(string filename, out string ext)
		{
			ext = string.Empty;

			if (string.IsNullOrEmpty(filename))
			{
				return "";
			}	

			var lastDot = filename.LastIndexOf('.');
			if (lastDot <= 0)
			{
				return "";
			}

			ext = filename.Substring(lastDot + 1);

			var name = filename.Substring(0, lastDot);

			return name;
		}

		/// <summary>
		/// truncate a string for short view
		/// </summary>
		/// <param name="input">a string to truncate</param>
		/// <param name="length">maximum length to view</param>
		/// <returns>a string with length less than length</returns>
		public static string Truncate(string input, int length=20)
		{
			if (string.IsNullOrEmpty(input))
			{
				return string.Empty;
			}

			if (input.Length < length)
			{
				return input;
			}

			var output = input.Substring(0, length) + "...";

			return output;
		}
	}
}