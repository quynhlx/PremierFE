using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using log4net;

namespace debt_fe.DataAccessHelper
{
    public class DataProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private SqlConnection _connection;

        private string _username;
        private string _password;

        public DataProvider()
        {

        }

        public DataProvider(string username, string password)
        {
            this._username = username;
            this._password = password;
        }

        private bool Connect()
        {
            var connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            connectionString = string.Format("{0}; User ID={1}; Password={2}", connectionString, _username, _password);

            if (_connection == null || _connection.State == ConnectionState.Closed)
            {
                _connection = new SqlConnection(connectionString);
            }


            return (_connection.State == ConnectionState.Open);
        }

        /// <summary>
        /// Execute non query command: select
        /// </summary>
        /// <param name="query">a string is select command</param>
        /// <returns>a DataTable of select query result</returns>
        public DataTable ExecuteNonQuery(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new Exception("Query not found");
            }

            if (!Connect())
            {
                throw new Exception("Cannot connect to database: " + _connection.ConnectionString);
            }

            var adapter = new SqlDataAdapter(query, _connection);
            var table = new DataTable();

            adapter.Fill(table);

            return table;
        }

        /// <summary>
        /// execute a select query with parameter
        /// </summary>
        /// <param name="query">a string of select query</param>
        /// <param name="paramNames">a list string of parameter names</param>
        /// <param name="paramValues">an arraylist of parameter values</param>
        /// <returns>a datatable of query result</returns>
        public DataTable ExecuteNonQuery(string query, List<string> paramNames, ArrayList paramValues)
        {
            var table = new DataTable();


            if (string.IsNullOrEmpty(query))
            {
                throw new Exception("Query not found");
            }

            if (!Connect())
            {
                throw new Exception("Cannot connect to database: " + _connection.ConnectionString);
            }


            var cmd = new SqlCommand(query, _connection);

            for (int i = 0; i < paramNames.Count; i++)
            {
                var name = paramNames[i].TrimStart('@');
                name = string.Format("@{0}",name);

                cmd.Parameters.AddWithValue(name, paramValues[i]);
            }

            var adapter = new SqlDataAdapter(cmd);

            try
            {
                adapter.Fill(table);
            }
            catch(Exception ex)
            {
                throw ex;
            }

            return table;
        }

        /// <summary>
        /// Execute query command: insert, update, delete
        /// </summary>
        /// <param name="query">a string of query command</param>
        public void ExecuteQuery(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new Exception("Query not found");
            }

            if (!Connect())
            {
                throw new Exception("Cannot connect to database: " + _connection.ConnectionString);
            }

            _connection.Open();
            var cmd = new SqlCommand(query, _connection);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection != null)
                {
                    _connection.Close();
                }

                if (cmd != null)
                {
                    cmd.Dispose();
                }
            }

        }

        /// <summary>
        /// Execute query command: insert, update, delete with parameter
        /// </summary>
        /// <param name="query">a string of query command</param>
        public void ExecuteQuery(string query, List<string> paramNames, ArrayList paramValues)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new Exception("Query not found");
            }

            if (!Connect())
            {
                throw new Exception("Cannot connect to database: " + _connection.ConnectionString);
            }

            _connection.Open();
            var cmd = new SqlCommand(query, _connection);            

            for (int i = 0; i < paramNames.Count; i++)
            {
                var name = paramNames[i].TrimStart('@');
                name = string.Format("@{0}", name);

                cmd.Parameters.AddWithValue(name, paramValues[i]);
            }

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection != null)
                {
                    _connection.Close();
                }

                if (cmd != null)
                {
                    cmd.Dispose();
                }
            }

        }

        /// <summary>
        /// Execute select store procedure
        /// </summary>
        /// <param name="storeProc">a string is store procedure name</param>
        /// <param name="paramNames">a string collection of parameters name without @</param>
        /// <param name="paramValues">an array list of parameters value</param>
        /// <returns>an object of store procedure result</returns>
        public object ExecuteStoreProcedure(string storeProc, List<string> paramNames, ArrayList paramValues)
        {
            object returnValue = null;

            if (string.IsNullOrEmpty(storeProc))
            {
                throw new Exception("Store procedure not found");
            }

            if (paramNames == null || paramNames.Count == 0)
            {
                throw new Exception("Parameters not found");
            }

            if (!Connect())
            {
                throw new Exception("Cannot connect to database: " + _connection.ConnectionString);
            }

            /*
             * prepare data
             * */
            var cmd = new SqlCommand(storeProc, _connection);
            cmd.CommandType = CommandType.StoredProcedure;

            for (int i = 0; i < paramNames.Count; i++)
            {
                cmd.Parameters.AddWithValue(paramNames[i], paramValues[i]);
            }

            var returnParam = cmd.Parameters.Add("@b", SqlDbType.NVarChar);
            returnParam.Direction = ParameterDirection.ReturnValue;

            /*
             * execute query
             * */
            try
            {
                _connection.Open();
                cmd.ExecuteNonQuery();

                returnValue = cmd.Parameters["@b"].Value;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection != null)
                {
                    _connection.Close();
                }

                if (cmd != null)
                {
                    cmd.Dispose();
                }
            }


            return returnValue;
        }

        /// <summary>
        /// Execute select store procedure
        /// </summary>
        /// <param name="storeProc">a string is store procedure name</param>
        /// <param name="paramNames">a string collection of parameters name without @</param>
        /// <param name="paramValues">an array list of parameters value</param>
        /// <returns>a dataset of store procedure result</returns>
        public DataSet ExecuteStoreProcedure2(string storeProc, List<string> paramNames, ArrayList paramValues)
        {
            //
            // TODO
            //
            // object returnValue = null;

            if (string.IsNullOrEmpty(storeProc))
            {
                throw new Exception("Store procedure not found");
            }

            if (paramNames == null || paramNames.Count == 0)
            {
                throw new Exception("Parameters not found");
            }

            if (!Connect())
            {
                throw new Exception("Cannot connect to database: " + _connection.ConnectionString);
            }

            /*
             * prepare data
             * */
            // var cmd = new SqlCommand(storeProc, _connection);
            var adapter = new SqlDataAdapter(storeProc, _connection);
            // cmd.CommandType = CommandType.StoredProcedure;

            //for (int i = 0; i < paramNames.Count; i++)
            //{
            //    cmd.Parameters.AddWithValue(paramNames[i], paramValues[i]);
            //}

            //var returnParam = cmd.Parameters.Add("@b", SqlDbType.NVarChar);
            //returnParam.Direction = ParameterDirection.ReturnValue;

            /*
             * execute query
             * */
            try
            {


                //_connection.Open();
                //cmd.ExecuteNonQuery();

                //returnValue = cmd.Parameters["@b"].Value;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_connection != null)
                {
                    _connection.Close();
                }

                //if (cmd != null)
                //{
                //    cmd.Dispose();
                //}
            }


            // return returnValue;
            return null;
        }
    }
}