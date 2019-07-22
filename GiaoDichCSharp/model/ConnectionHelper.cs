using System.Data;
using MySql.Data.MySqlClient;

namespace GiaoDichCSharp.model
{
    public class ConnectionHelper
    {
        private const string ServerName = "localhost";
        private const string DatabaseName = "giaodich";
        private const string Username = "root";
        private const string Password = "";
        private static MySqlConnection _connection;

        public static MySqlConnection GetConnect()
        {
            if (_connection == null)
            {
                _connection = new MySqlConnection($"Server={ServerName};Database={DatabaseName};Uid={Username};Pwd={Password};SslMode=none");
                _connection.Open();
            }
            else if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }
            return _connection;
        }
        public static void CloseConnect()
        {
            if (_connection == null || _connection.State == ConnectionState.Closed)
            {
                return;
            }
            _connection.Close();
        }
    }
}