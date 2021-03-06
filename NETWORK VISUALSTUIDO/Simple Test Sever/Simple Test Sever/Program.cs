﻿using System;
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


            int listenPort;
            Console.WriteLine("Local IP is: "+localIP);
            string temp = "137";
            try
            {
                listenPort = Convert.ToInt32(temp);
            }
            catch
            {
                Console.WriteLine("invalid Port, Using port 137 for default");
                listenPort = 137;
            }
            
            int recv;
            IPEndPoint ipep;
            byte[] data = new byte[1024];
            byte[] msg = new byte[1024];
            string stMsg;
            try
            {
                 ipep = new IPEndPoint(IPAddress.Any, listenPort);
            }
            catch
            {
                Console.WriteLine("An error occured on that port. Using port 137 as default.");
                ipep = new IPEndPoint(IPAddress.Any, 137);
            }

            

            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            newsock.Bind(ipep);
            newsock.Listen(10);
            Console.WriteLine("Waiting for client....");
            Socket client = newsock.Accept();
            IPEndPoint clientep = (IPEndPoint)client.RemoteEndPoint;

            Console.WriteLine("Connected with {0} at port {1}", clientep.Address, clientep.Port);

            string Welcome = "Please type your Message to the client.";
            data = Encoding.ASCII.GetBytes(Welcome);
            client.Send(data, data.Length, SocketFlags.None);

            while (true)
            {
                data = new byte[1024];
                try
                {
                    recv = client.Receive(data);
                    if (recv == 0)
                        break;
                }
                catch
                {
                    break;
                }


                Console.WriteLine(clientep.Address+": "+Encoding.ASCII.GetString(data, 0, recv));
                if(Encoding.ASCII.GetString(data,0,recv).Equals("Closing connection"))
                {
                    break;
                }
                else
                {
                    Console.Write(localIP + ": ");
                }
                

                msg = new byte[1024];
                stMsg = Console.ReadLine();
                while (String.IsNullOrEmpty(stMsg))
                {
                    Console.WriteLine("That message was blank please type a messge");
                    Console.Write(localIP + ": ");
                    stMsg = Console.ReadLine();

                }
                msg = Encoding.ASCII.GetBytes(stMsg);

                client.Send(msg, msg.Length, SocketFlags.None);
            }

            Console.WriteLine("Disconnected from client {0}", clientep.Address);

            client.Close();
            newsock.Close();
            Console.ReadLine();
             
        }
    }
}
