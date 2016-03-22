using System;
using System.Data.SqlClient;

namespace DDC.Autotests.Framework
{
    public class DbOperations
    {
        private SqlConnection _conn;

        public DbOperations(string connectionString)
        {
            _conn = new SqlConnection(connectionString);
            try
            {
                _conn.Open();
            }
            catch
            {
                throw new Exception("Can't open a connection");
            }

        }

        public string GetState()
        {
            return _conn.State.ToString();
        }

        public SqlConnection GetConnection()
        {
            return _conn;
        }
    }
}