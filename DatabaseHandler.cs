using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Data;
using MySql.Data.MySqlClient;

namespace EleunameBotConsole
{
    public class DatabaseHandler
    {

        public static MySqlConnection connection;
        public static string myConnectionString = "server=;database=;uid=;pwd=;SslMode=none"; //deleted all my data for security purpose from this string but that's the order.

       public static void ExecuteNonQuery(string Query)
        {
            connection = new MySqlConnection(myConnectionString);
            MySqlCommand cmd = new MySqlCommand(Query, connection);
            connection.Open();
            cmd.ExecuteNonQuery();
            connection.Close();
        }
        public static string ScalarCommand(string ScalarQuery)
        {
            string result;
            connection = new MySqlConnection(myConnectionString);
            MySqlCommand myCommand = new MySqlCommand(ScalarQuery, connection);
            connection.Open();
            result = myCommand.ExecuteScalar().ToString();
            connection.Close();
            return result;
        }
    }
}
