using Newtonsoft.Json;
using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Server
{
    class DataBaseWork
    {
        SQLiteConnection conn;
        private void init()
        {
            conn = new SQLiteConnection(@"Data Source=MyDataBase.db");
            conn.Open();
            
        }
        public DataBaseWork()
        {
            init();
        }
        public void WriteRequest(string request)
        {
            SQLiteCommand cmd = new SQLiteCommand(conn);
            cmd.CommandText = request;
            cmd.ExecuteScalar();
        }
        public List<Dictionary<string, object>> ReadReqest(string input)
        {
            SQLiteCommand cmd = new SQLiteCommand(conn);

            cmd.CommandText =input;
            SQLiteDataReader reader = cmd.ExecuteReader();

            var temp = new List<Dictionary<string, object>>(); 
            while(reader.Read())
                temp.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue));
            
            reader.Close();
            return temp;

        }
    }
    static class LogIn
    {
        public static string check(string username, string password)
        {
            DataBaseWork work = new DataBaseWork();
            var x = work.ReadReqest($"SELECT * FROM clerks WHERE UserName='{username}' AND Password='{password}'");
            if (x.Count > 0)
            {
                return "True";
            }
            return "False";
            
        }
    }

    static class ClerkHandler
    {
        private static bool check_admin(string username, string password)
        {
            DataBaseWork work = new DataBaseWork();
            var x = work.ReadReqest($"SELECT * FROM clerks WHERE UserName='{username}' AND Password='{password}'");
            return (long)(x[0]["IsAdmin"]) == 1;
        }
        public static string check(ClientToServer item)
        {

            if (!check_admin(item.UserName, item.Password))
            {
                return "Not Admin";
            }
            return "None";
        }
    }
    class RequstHandler
    {
        public string requst(string input)
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            };
            ClientToServer item = (ClientToServer)JsonConvert.DeserializeObject(input, settings);
            if (!item.Apply && !item.Select) return LogIn.check(item.UserName, item.Password);
            else if (item.clerk) return ClerkHandler.check(item);
            return "Nothing";
        }
    }
}
