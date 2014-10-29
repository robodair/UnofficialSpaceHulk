using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Simple_Test_Sever
{
    class Program
    {
        static void Main(string[] args)
        {
            int recv;
            byte[] data = new byte[1024];
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 139);


            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            newsock.Bind(ipep);
            newsock.Listen(10);
            Console.WriteLine("Waiting for client....");
            Socket client = newsock.Accept();
            IPEndPoint clientep = (IPEndPoint)client.RemoteEndPoint;

            Console.WriteLine("Connected with {0} at port {1}", clientep.Address, clientep.Port);

            string Welcome = "welcome to my test Server";
            data = Encoding.ASCII.GetBytes(Welcome);
            client.Send(data, data.Length, SocketFlags.None);

            while (true)
            {
                data = new byte[1024];

                recv = client.Receive(data);
                if (recv == 0)
                    break;


                Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));
                client.Send(data, recv, SocketFlags.None);
            }

            Console.WriteLine("Disconnected from client {0}", clientep.Address);

            client.Close();
            newsock.Close();
            Console.ReadLine();
        }
    }
}
