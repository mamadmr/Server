using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class MySocket
    {
        IPHostEntry host;
        IPAddress ipAddress;
        IPEndPoint localEndPoint;
        Socket listener;
        private void init()
        {
            try
            {
                host = Dns.GetHostEntry("localhost");
                ipAddress = host.AddressList[0];
                localEndPoint = new IPEndPoint(ipAddress, 11000);

                listener = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(10);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public MySocket()
        {
            init();
        }
        public void Read()
        {
           
            Console.WriteLine("waiting. . . ");
            Socket handler = listener.Accept();
            string data = null;
            byte[] bytes = null;

            while (true)
            {
                bytes = new byte[1024];
                int bytesRec = handler.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                if (data.IndexOf("<EOF>") > -1)
                {
                    break;
                }
            }
            Console.WriteLine(data);

            byte[] msg = Encoding.ASCII.GetBytes("everything is good");
            handler.Send(msg);
        }
    }
}
