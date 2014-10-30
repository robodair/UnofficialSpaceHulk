using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Simple_tcp_Client
{
    class Program
    {
        static void Main(string[] args)
        {



            //Find own local IP address
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            byte[] data = new byte[1024];
            string input, stringData;
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("172.26.186.29"), 137);

            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


            try
            {

                server.Connect(ipep);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Unable to connect to the server.");
                Console.WriteLine(e.ToString());

                return;
            }

            int recv = server.Receive(data);
            stringData = Encoding.ASCII.GetString(data, 0, recv);
            Console.WriteLine(stringData);

            while (true)
            {
                Console.Write(localIP+": ");
                input = Console.ReadLine();
                if (input == "\\exit")
                    break;
                server.Send(Encoding.ASCII.GetBytes(input));
                data = new byte[1024];
                try
                {
                    recv = server.Receive(data);

                    stringData = Encoding.ASCII.GetString(data, 0, recv);
                    Console.WriteLine(ipep.Address + ": " + stringData);
                }
                catch
                {
                    Console.WriteLine("Server disconnected.");
                    Console.ReadLine();
                    break;
                }

            }
            Console.WriteLine("Disconnecting from server");
            server.Shutdown(SocketShutdown.Both);
            server.Close();


        }
    }
}
