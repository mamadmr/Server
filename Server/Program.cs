using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {
        public void Run()
        {
            MySocket mySocket = new MySocket();
            while (true)
            {
                mySocket.Read();
            }
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Run();
        }
    }
}
