using Newtonsoft.Json;
using System.Data.SQLite;

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
        public string ReadReqest(string input)
        {
            SQLiteCommand cmd = new SQLiteCommand(conn);

            cmd.CommandText =input;
            SQLiteDataReader reader = cmd.ExecuteReader();

            string temp =  reader.Read().ToString();
            reader.Close();
            return temp;

        }
    }
    static class LogIn
    {
        public static string check(string username, string passoword)
        {
            DataBaseWork work = new DataBaseWork();
            return work.ReadReqest($"SELECT * FROM clerks WHERE UserName='{username}' AND Password='{passoword}'");
            
        }
    }
    class RequstHandler
    {
        public string requst(string input)
        {
            ClientToServer item = JsonConvert.DeserializeObject<ClientToServer>(input);
            if (!item.Apply && !item.Select) return LogIn.check(item.UserName, item.Password);
            return "Nothing";
        }
    }
}
