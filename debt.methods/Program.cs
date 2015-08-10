using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;

namespace debt.methods
{
	class Program
	{
		static void Main(string[] args)
		{

            var pass = "1";
            var md5 = ToMD5Hash(pass);

			var contacts = new List<ContactInformationModel>
			{
				new ContactInformationModel
				{
					Id=1,
					HomePhone="123-456-789",
					WorkPhone="",
					MobilePhone="000-111-222",
					Email="oclockvn@gmail.com",
					City="Ho Chi Minh",
					Zip="123456",
					State="CA",
					Preferred=1,
					BestTimeToContact="10 AM"
				},
				new ContactInformationModel
				{
					Id=2,
					HomePhone="123-456-789",
					WorkPhone="",					
					State="CA",
					Preferred=1,
					BestTimeToContact="10 AM"
				},
				new ContactInformationModel
				{
					Id=3,
					HomePhone="123-456-789",
					WorkPhone="",
					MobilePhone="000-111-222",
					Email="oclockvn@gmail.com",					
					BestTimeToContact="10 AM"
				}
			};

			using (XmlWriter writer = XmlWriter.Create("contacts3.xml"))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("UpdateHistory");

				

				foreach (ContactInformationModel contact in contacts)  // <-- This is new
				{

					writer.WriteStartElement("Update"); // <-- Write employee element
					

					writer.WriteElementString("UpdatedBy", "tienquang");
					writer.WriteElementString("UpdatedDate", DateTime.Now.ToString());

					writer.WriteStartElement("UpdateDetail"); // <-- Write employee element

					writer.WriteElementString("ID", contact.Id.ToString());   // <-- These are new
					writer.WriteElementString("HomePhone", contact.HomePhone);
					writer.WriteElementString("WorkPhone", contact.WorkPhone);
					writer.WriteElementString("MobilePhone", contact.MobilePhone);
					writer.WriteElementString("Address", contact.StreetAddress);
					writer.WriteElementString("City", contact.City);
					writer.WriteElementString("State", contact.State);
					writer.WriteElementString("Zip", contact.Zip);
					writer.WriteElementString("Email", contact.Email);
					writer.WriteElementString("BestTimeToContact", contact.BestTimeToContact);
					writer.WriteElementString("Preferred", contact.Preferred.ToString());


					writer.WriteEndElement();

					writer.WriteEndElement();
				}

				writer.WriteEndElement();
				writer.WriteEndDocument();
			}

			return;

            var p = Directory.GetLogicalDrives()[0];
            var p1 = Environment.GetLogicalDrives()[0];
            var p2 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            var path2 = Path.GetDirectoryName("C");
            var path3 = Environment.SpecialFolder.MyComputer;

			



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

	[Serializable()]
	public class ContactInformationModel
	{
		public int Id { get; set; }
		public string HomePhone { get; set; }
		public string WorkPhone { get; set; }
		public string MobilePhone { get; set; }
		public int Preferred { get; set; }
		/*
		 * preferred code
		 * 
		 * 1: home phone
		 * 2: work phone
		 * 3: mobile phone
		 * 
		 * */
		public string Email { get; set; }
		public string BestTimeToContact { get; set; }
		public string StreetAddress { get; set; }
		public string City { get; set; }
		public string Zip { get; set; }
		public string State { get; set; }
	}

	public class ContactInformationUpdateHistoryModel
	{
		public string UpdatedBy { get; set; }
		public DateTime UpdatedDate { get; set; }
		public int? ContactInformationId { get; set; }
	}
}
