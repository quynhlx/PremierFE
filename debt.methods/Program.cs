using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

namespace debt.methods
{
	class Program
	{
		static void Main(string[] args)
		{
            var path2 = Path.GetDirectoryName("C");
            var path3 = Environment.SpecialFolder.MyComputer;

			var pass = "1";
			var md5 = ToMD5Hash(pass);



			var path = "~//asd/~/asd";
			path = path.TrimStart('~', '/');

			var model = new Model
			{
				ID = 1,
				Name = "tienquang",
				Desc = "oclockvn"
			};

			var hash = new Hashtable();
			hash.Add(model.ID, model.ID);
			var s = model.ID.GetTypeCode().ToString();
			var n = model.ID.GetType().Name;
			var l = model.GetType().GetProperty("ID").Name;
			hash.Add(model.Name, model.Name);

			foreach (DictionaryEntry item in hash)
			{
				var key = item.Key.ToString();
				var val = item.Value.ToString();

				// hash.GetType()

			}

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

	class Model
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public string Desc { get; set; }
	}
}
