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
using System.ComponentModel;

namespace Server
{
    public static class Handler
    {
        public static bool check_user(string username, string password)
        {
            DataBaseWork work = new DataBaseWork();
            var x = work.ReadReqest($"SELECT * FROM clerks WHERE UserName='{username}' AND Password='{password}'");
            return x.Count >= 1;
        }
    }
    class DataBaseWork
    {
        SQLiteConnection conn;
        private void init()
        {
            string temp = System.Reflection.Assembly.GetExecutingAssembly().Location;
            temp = temp.Substring(0, temp.Length - 20);
            conn = new SQLiteConnection($"Data Source={temp}MyDataBase.db");
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
            try
            {
                cmd.CommandText = input;
                SQLiteDataReader reader = cmd.ExecuteReader();

                var temp = new List<Dictionary<string, object>>();
                while (reader.Read())
                    temp.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue));

                reader.Close();
                return temp;
            }
            catch (Exception ex)
            {
                throw ex;
            }

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
                        try
                        {
                            var x = work.ReadReqest($"INSERT INTO clerks (Name, PhoneNumber, Address, UserName, Password, IsAdmin)" +
                                $"VALUES('{clr.Name}', '{clr.PhoneNumber}', '{clr.Address}', '{clr.UserName}', '{clr.Password}', {temp})");
                        }
                        catch
                        {
                            return "User name was taken";
                        }
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
                            $"Password='{clr.Password}'" +
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
    static class CakeHandler
    {
        public static string check(ClientToServer item)
        {
            if (!Handler.check_user(item.UserName, item.Password)) return "your username or password has been expired";
            if (item.Apply)
            {
                foreach (Cake x in item.Objects)
                {
                    DataBaseWork work = new DataBaseWork();
                    if (x.IsNew)
                    {
                        var db = work.ReadReqest($"INSERT INTO cakes (Name, Price, Description)" +
                         $"VALUES('{x.Name}',{x.Price}, '{x.Description}')");
                    }
                    else if (x.Removed)
                    {
                        var db = work.ReadReqest($"DELETE FROM cakes WHERE Id={x.Id}");
                    }
                    else
                    {
                        var db = work.ReadReqest($"UPDATE cakes " +
                                                $"SET " +
                                                $"Name='{x.Name}'," +
                                                $"Price= {x.Price}," +
                                                $"Description='{x.Description}' " +
                                                $"WHERE Id={x.Id}");
                    }
                }
                return "Done";
            }
            else
            {
                Cake cls = (Cake)item.SelectObject;
                DataBaseWork work = new DataBaseWork();
                var x = work.ReadReqest($"SELECT * FROM cakes WHERE " +
                                        $"(Name='{cls.Name}' OR '{cls.Name}'='') AND " +
                                        $"(Price={cls.Price} OR {cls.Price}=0) AND " +
                                        $"(Description='{cls.Description}' OR '{cls.Description}'='')");

                ServerToClient answer = new ServerToClient();
                answer.Objects = new List<ISendAble>();

                foreach (var selected in x)
                {
                    ISendAble cler = new Cake((long)selected["Price"], (string)selected["Name"],
                                                (string)selected["Description"]);
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
    
    static class CustomerHandler
    {
        public static string check(ClientToServer item)
        {
            if (!Handler.check_user(item.UserName, item.Password)) return "your username or password has been expired";
            if (item.Apply)
            {
                foreach (Customer x in item.Objects)
                {
                    DataBaseWork work = new DataBaseWork();
                    if (x.IsNew)
                    {
                        var db = work.ReadReqest($"INSERT INTO customers (Name, PhoneNumber, Address, Money, OrderCountRecieve, OrderCountRemove, SubscribeCode)" +
                         $"VALUES('{x.Name}','{x.PhoneNumber}', '{x.Address}' , {x.Balance}, {x.OrderCountRecive}, {x.OrderCountRemove}, {x.SubscribeCode})");
                    }
                    else if (x.Removed)
                    {
                        var db = work.ReadReqest($"DELETE FROM customers WHERE Id={x.Id}");
                    }
                    else
                    {
                        var db = work.ReadReqest($"UPDATE customers " +
                                                $"SET " +
                                                $"Name='{x.Name}'," +
                                                $"PhoneNumber= {x.PhoneNumber}, " +
                                                $"Address='{x.Address}', " +
                                                $"Money={x.Balance} " +
                                                $"WHERE Id={x.Id}");
                    }
                }
                return "Done";
            }
            else
            {
                Customer cls = (Customer)item.SelectObject;
                DataBaseWork work = new DataBaseWork();
                long temp = 0;
                if (cls.SubscribeCode != "") temp = Int64.Parse(cls.SubscribeCode);
                
                var x = work.ReadReqest($"SELECT * FROM customers WHERE " +
                                        $"(Name='{cls.Name}' OR '{cls.Name}'='') AND " +
                                        $"(Money={cls.Balance} OR {cls.Balance}=0) AND " +
                                        $"(PhoneNumber='{cls.PhoneNumber}' OR '{cls.PhoneNumber}'='') AND" +
                                        $"(SubscribeCode={temp} OR {temp}=0)");

                ServerToClient answer = new ServerToClient();
                answer.Objects = new List<ISendAble>();

                foreach (var selected in x)
                {
                    ISendAble cler = new Customer((string)selected["Name"], (string)selected["PhoneNumber"], (string)selected["Address"],
                        selected["SubscribeCode"].ToString());
                    ((Customer)cler).Balance = (long)selected["Money"];
                    ((Customer)cler).OrderCountRecive = (long)selected["OrderCountRecieve"];
                    ((Customer)cler).OrderCountRemove = (long)selected["OrderCountRemove"];

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
    
    static class OrderHandler
    {
        static System.Random random;

        public static bool check_money(long Id, long money)
        {
            DataBaseWork work = new DataBaseWork();
            var db = work.ReadReqest($"SELECT * FROM customers WHERE Id={Id}");
            foreach(var x in db) return (long)x["Money"]>=money;
            return false;
        }
        
        public static long product_cout(string input)
        {
            long count = 0;
            foreach (Item item in JsonConvert.DeserializeObject<List<Item>>(input))
            {
                count += item.count;
            }
            return count;
        }
        public static Dictionary<long, long>  check_hour()
        {
            DataBaseWork work = new DataBaseWork();
            Dictionary<long , long> hours = new Dictionary<long , long>();
            for (int i = ConstValue.start_hour; i<=ConstValue.end_hour; i++)
            {
                hours.Add(i, 0);
                var db = work.ReadReqest($"SELECT * FROM orders WHERE Hour={i}");
                foreach(var x in db)
                {
                    hours[i] += product_cout((string)x["Products"]);
                }
            }
            return hours;
        }

        static private string code_generator()
        {
            return random.Next(1000, 99999).ToString();
        }
        static public string check(ClientToServer item)
        {
            random = new System.Random();

            if (!Handler.check_user(item.UserName, item.Password)) return "your username or password has been expired";
            if (item.Apply)
            {
                foreach (Order x in item.Objects)
                {
                    DataBaseWork work = new DataBaseWork();
                    if (x.IsNew)
                    {

                        string custmoer_str = JsonConvert.SerializeObject(x.Customer);
                        string products_str = JsonConvert.SerializeObject(x.Products);
                        if (!check_money(x.Customer.Id, x.TotalPrice)) return "Not Enoungh Money";
                        if (check_hour()[x.Hour] + x.Products.Count > ConstValue.max_in_hour)
                        {
                            string temp = "this Hour is full\nyou can choose between: ";
                            for (int i = ConstValue.start_hour; i <= ConstValue.end_hour; i++)
                            {
                                if (check_hour()[i] + x.Products.Count <= ConstValue.max_in_hour) temp += i.ToString() + " ";
                            }
                            return temp;
                        }
                        work.WriteRequest($"Update customers SET Money=Money-{x.TotalPrice} WHERE Id={x.Customer.Id}");
                        x.OrderCode = code_generator();
                        var db = work.ReadReqest($"INSERT INTO orders (OrderCode, Hour, TotalPrice,Products, Customer)" +
                         $"VALUES('{x.OrderCode}',{x.Hour}, {x.TotalPrice} , '{products_str}', '{custmoer_str}')");
                    }
                    else if (x.Removed)
                    {
                        var dc = work.ReadReqest($"SELECT * FROM orders WHERE Id={x.Id}");
                        work.ReadReqest($"DELETE FROM orders WHERE Id={x.Id}");
                        try
                        {
                            work.WriteRequest($"Update customers SET Money=Money+{(long)((dc[0])["TotalPrice"])} WHERE Id={x.Customer.Id}");
                        }
                        catch
                        {
                            return "Not availble to give money back";
                        }
                    }
                    else
                    {
                        string custmoer_str = JsonConvert.SerializeObject(x.Customer);
                        string products_str = JsonConvert.SerializeObject(x.Products);
                        var dc = work.ReadReqest($"SELECT * FROM orders WHERE Id={x.Id}");
                        long temp_money = x.TotalPrice - (long)dc[0]["TotalPrice"];
                        if (!check_money(x.Customer.Id, temp_money)) return "Not Enough Money";
                        if ((long)dc[0]["Hour"] != x.Hour)
                        {
                            if (check_hour()[x.Hour] + x.Products.Count > ConstValue.max_in_hour)
                            {
                                string temp = "this Hour is full\nyou can choose between: ";
                                for (int i = ConstValue.start_hour; i <= ConstValue.end_hour; i++)
                                {
                                    if (check_hour()[i] + x.Products.Count <= ConstValue.max_in_hour) temp += i.ToString() + " ";
                                }
                                return temp;
                            }
                        }

                        work.WriteRequest($"Update customers SET Money=Money-{temp_money} WHERE Id={x.Customer.Id}");

                        var db = work.ReadReqest($"UPDATE orders " +
                                                $"SET " +
                                                $"OrderCode='{x.OrderCode}'," +
                                                $"Hour= {x.Hour}, " +
                                                $"TotalPrice={x.TotalPrice}, " +
                                                $"Products='{products_str}'," +
                                                $"Customer='{custmoer_str}' " +
                                                $"WHERE Id={x.Id}");
                    }
                }
                return "Done";
            }
            else
            {

                Order cls = (Order)item.SelectObject;
                DataBaseWork work = new DataBaseWork();
                string custmoer_str = JsonConvert.SerializeObject(cls.Customer);
                if (cls.Customer == null) custmoer_str = "";
                var x = work.ReadReqest($"SELECT * FROM orders WHERE " +
                                        $"(Hour={cls.Hour} OR {cls.Hour}=0) AND " +
                                        $"(Id={cls.Id} OR {cls.Id}=0) AND " +
                                        $"(OrderCode='{cls.OrderCode}' OR '{cls.OrderCode}'='') AND" +
                                        $"(Customer='{custmoer_str}' OR '{custmoer_str}'='')");

                ServerToClient answer = new ServerToClient();
                answer.Objects = new List<ISendAble>();

                foreach (var selected in x)
                {

                    ISendAble cler = new Order();
                    foreach (var jj in JsonConvert.DeserializeObject<List<Item>>((string)selected["Products"]))
                    {
                        ((Order)cler).Products.Add(jj);
                    }

                    ((Order)cler).Customer = JsonConvert.DeserializeObject<Customer>((string)selected["Customer"]);
                    cler.Id = (long)selected["Id"];
                    ((Order)cler).Hour = Int32.Parse(selected["Hour"].ToString());
                    ((Order)cler).OrderCode = (string)selected["OrderCode"];

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
            else if (item.product) return CakeHandler.check(item);
            else if(item.cutomer) return CustomerHandler.check(item);
            else if(item.order) return OrderHandler.check(item);
            return "Nothing";
        }
    }
}
