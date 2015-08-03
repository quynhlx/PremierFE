using debt_fe.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;

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
        /// format file name with random string by System.IO.Path
        /// </summary>
        /// <param name="filename">a string of filename like my-file.ext</param>
        /// <returns>a string kind of my-file-random-string.ext</returns>
        public static string FormatFileNameByPath(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return "";
            }

            var ext = System.IO.Path.GetExtension(filename);
            var name = System.IO.Path.GetFileNameWithoutExtension(filename);
            var randomString = Path.GetRandomFileName().Replace(".", string.Empty);

            if (string.IsNullOrEmpty(name))
            {
                return "";
            }

            var formatted = string.Format("{0}-{1}{2}", name, randomString, ext);

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

        /// <summary>
        /// convert a hashtable to string
        /// </summary>
        /// <param name="hash">hastable to convert</param>
        /// <returns>a string in format [key=value]</returns>
        public static string HashtableToString(Hashtable hash)
        {
            var builder = new StringBuilder();

            foreach (DictionaryEntry entry in hash)
            {
                builder.AppendFormat("{0}[{1}={2}]", Environment.NewLine,entry.Key, entry.Value);
            }

            return builder.ToString();
        }

        public static string GetDocumentsPath(int DocumentISN, object AddedDate)
        {
            /*
            WebControlLibrary.Utility.Database objDB = new WebControlLibrary.Utility.Database(System.Configuration.ConfigurationSettings.AppSettings["ReportConnectionString"]);
            DateTime date = DateTime.Now;
            if (AddedDate == null)
                AddedDate = objDB.ExecuteScalar("Select docAddedDate From Document Where DocumentISN=" + DocumentISN.ToString());

            if (AddedDate != null) date = Convert.ToDateTime(AddedDate);
            string strPath = date.ToString("yyyyMM") + "\\" + "Documents\\" + date.ToString("dd") + "\\" + DocumentISN.ToString();
            return strPath;
             */
            return "";
        }

        public static string GetColumnValue(DataRow row, string columnName)
        {
            var value = string.Empty;

            if (string.IsNullOrEmpty(columnName) || string.IsNullOrWhiteSpace(columnName))
            {
                return value;
            }

            if (row.Table.Rows.Count==0 || row.Table.Columns.Count==0 || row ==null)
            {
                return value;
            }

            try
            {
                value = row[columnName].ToString();
            }
            catch (Exception)
            {                
                return string.Empty;
            }

            return value;
        }

        public static DataSet ConvertXMLToDataSet(string xmlData)
        {
            if (string.IsNullOrEmpty(xmlData))
            {
                return null;
            }

            
            using (var stream = new StringReader(xmlData))
            {
                using (var reader = new XmlTextReader(stream))
                {
                    var ds = new DataSet();

                    try
                    {
                        ds.ReadXml(reader);

                        return ds;
                    }
                    catch(Exception ex)
                    {
                        throw ex;
                    }
                }
            }

           /*
            StringReader stream = null;
            XmlTextReader reader = null;
            try
            {
                DataSet xmlDS = new DataSet();
                stream = new StringReader(xmlData);
                // Load the XmlTextReader from the stream
                reader = new XmlTextReader(stream);
                xmlDS.ReadXml(reader);
                return xmlDS;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            */ 
            
        }

        /// <summary>
        /// check if table contain column with name
        /// </summary>
        /// <param name="columnName">a string of column name</param>
        /// <param name="table">a table to check</param>
        /// <returns>true if table contain column name</returns>
        public static bool ColumnExist(string columnName, DataTable table)
        {
            if (table == null || table.Columns.Count==0)
            {
                return false;
            }

            return table.Columns.Contains(columnName);
        }

        /// <summary>
        /// check if table contain list of columns
        /// </summary>
        /// <param name="columnNames">a list string of column names</param>
        /// <param name="table">a table to check</param>
        /// <param name="notExistColumnName">first column that does not exist in table</param>
        /// <returns>false if one of column does not exist in table</returns>
        public static bool ColumnsExist(List<string> columnNames, DataTable table, out string notExistColumnName)
        {
            notExistColumnName = string.Empty;            

            var exist = true;

            foreach (var columnName in columnNames)
            {
                if (!ColumnExist(columnName, table))
                {
                    notExistColumnName = columnName;

                    return false;
                }
            }

            return exist;
        }

		/// <summary>
		/// get state depend on input xml
		/// </summary>
		/// <param name="xml">a string of xml states</param>
		/// <returns>a list of state model</returns>
		public static List<StateModel> GetStates()
		{
			//
			// todo: get from xml
			// var states = new List<StateModel>();

			var xml = string.Empty;
			var path = HttpContext.Current.Server.MapPath("~/App_Data/USAState.xml");

			//
			// catch error
			using (var reader = new StreamReader(path))
			{
				xml = reader.ReadToEnd();
			}

			if (string.IsNullOrEmpty(xml))
			{
				return null;
			}

			var ds = ConvertXMLToDataSet(xml);

			if (ds == null || ds.Tables.Count==0)
			{
				return null;
			}

			var tableStates = ds.Tables["State"];

			if (tableStates.Rows.Count==0)
			{
				return null;
			}

			var states = new List<StateModel>();

			foreach (DataRow row in tableStates.Rows)
			{
				var state = new StateModel
				{
					Code = row["value"].ToString(),
					Name = row["text"].ToString()
				};

				states.Add(state);
			}

			return states;
		}
	}
}