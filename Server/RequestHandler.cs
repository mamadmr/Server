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
            if (item.Apply)
            {
                foreach (Clerk clr in item.Objects)
                {
                    string temp = "0";
                    if (clr.IsAdmin) temp = "1";
                    DataBaseWork work = new DataBaseWork();
                    if (clr.IsNew)
                    {
                        var x = work.ReadReqest($"INSERT INTO clerks (Name, PhoneNumber, Address, UserName, Password, IsAdmin)" +
                            $"VALUES('{clr.Name}', '{clr.PhoneNumber}', '{clr.Address}', '{clr.UserName}', '{clr.Password}', {temp})");
                    }
                    else if (clr.Removed)
                    {
                        var x = work.ReadReqest($"DELETE FROM clerks WHERE Id={clr.Id}");
                    }
                    else
                    {
                        var x = work.ReadReqest($"UPDATE clerks " +
                            $"SET " +
                            $"Name='{clr.Name}'," +
                            $"PhoneNumber='{clr.PhoneNumber}'," +
                            $"Address='{clr.Address}'," +
                            $"UserName='{clr.UserName}'," +
                            $"Password='{temp}'" +
                            $"WHERE Id={clr.Id}");
                    }
                }
                return "Done";
            }
            else
            {
                Clerk cls = (Clerk)item.SelectObject;
                DataBaseWork work = new DataBaseWork();
                string temp = "0";
                if (cls.IsAdmin) temp = "1";
                var x = work.ReadReqest($"SELECT * FROM clerks WHERE " +
                    $"(Password='{cls.Password}' OR '{cls.Password}'='') AND " +
                    $"(Name='{cls.Name}' OR '{cls.Name}'='') AND " +
                    $"(PhoneNumber='{cls.PhoneNumber}' OR '{cls.PhoneNumber}'='') AND " +
                    $"(UserName='{cls.UserName}' OR '{cls.UserName}'='') AND " +
                    $"IsAdmin={temp}");
                
                ServerToClient answer = new ServerToClient();
                answer.Objects = new List<ISendAble>();
                foreach(var selected in x)
                {
                    ISendAble cler = new Clerk((string)selected["Name"], (string)selected["PhoneNumber"],
                                                (string)selected["Address"],
                                                (string)selected["UserName"], (string)selected["Password"], 
                                                ((long)selected["IsAdmin"] == 1));
                    cler.Id = (long)selected["Id"];
                    answer.Objects.Add(cler);
                }
                MySocket mySocket = new MySocket();
                var indented = Formatting.Indented;
                var settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                string data = JsonConvert.SerializeObject(answer, indented, settings);
                return data;
            }
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
