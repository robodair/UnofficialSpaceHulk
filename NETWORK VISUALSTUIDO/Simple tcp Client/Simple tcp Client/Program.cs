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

            byte[] data = new byte[1024];
            string input, stringData;
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("172.26.187.8"), 139);



        }
    }
}
