using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Net.NetworkInformation;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace NetworkTesting
{
    class Program
    {
    
        
    
   



        private static List<System.Net.NetworkInformation.Ping> pingers = new List<System.Net.NetworkInformation.Ping>();
        private static int instances = 0;
        private static object @lock = new object();

        private static int result = 0;
        private static int timeOut = 250;

        private static int ttl = 5;
        public static void Main(string[] args)
        {




            

            TcpClient client = new TcpClient();

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("172.26.102.17"), 139);

            client.Connect(serverEndPoint);

            NetworkStream clientStream = client.GetStream();

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes("Hello Server!");

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();


            /*


            string destIP = "172.26.184.65";


            //Connection!
            IPAddress IP = IPAddress.Parse("127.0.0.1");

            if (IPAddress.TryParse(destIP, out IP))
            {
                Socket s = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

                try
                {
                    s.Connect(IP, 139);
                    Console.WriteLine("it worked???");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Console.ReadLine();
                }
            }








            bool pingable = false;
            System.Net.NetworkInformation.Ping pinger = new System.Net.NetworkInformation.Ping();

            try
            {
                PingReply reply = pinger.Send("172.26.102.17");

                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            Console.WriteLine("Ping was " + pingable.ToString());

            Console.WriteLine("Press enter to close...");
            Console.ReadLine();

            /*

            string baseIP = "172.26.102";
            Console.WriteLine("Step 55");
            Console.WriteLine("Pinging 255 destinations of D-class in  " + baseIP.ToString());
            Console.WriteLine("Press enter to cContine...");
            Console.ReadLine();
            CreatePingers(255);

            PingOptions po = new PingOptions(ttl, true);
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            byte[] data = enc.GetBytes("abababababababababababababababab");


            int cnt = 1;



            foreach (System.Net.NetworkInformation.Ping p in pingers)
            {
                lock (@lock)
                {
                    instances += 1;
                }

                p.SendAsync(string.Concat(baseIP, cnt.ToString()), timeOut, data, po);
                cnt += 1;
            }




            DestroyPingers();

            Console.WriteLine("Found active IP-addresses. " + result.ToString());
            Console.WriteLine("Press enter to cContine...");
            Console.ReadLine();


        }

        public static void Ping_completed(object s, PingCompletedEventArgs e)
        {
            lock (@lock)
            {
                instances -= 1;
            }

            if (e.Reply.Status == IPStatus.Success)
            {
                Console.WriteLine(string.Concat("Active IP: ", e.Reply.Address.ToString()));
                result += 1;
            }
            else
            {
                //Debug.Log(String.Concat("Non-active IP: ", e.Reply.Address.ToString()))
            }
            Console.WriteLine("Press enter to cContine...");
            Console.ReadLine();
        }


        private static void CreatePingers(int cnt)
        {
            for (int i = 1; i <= cnt; i++)
            {
                System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();
                p.PingCompleted += Ping_completed;
                pingers.Add(p);
            }
        }

        private static void DestroyPingers()
        {
            foreach (System.Net.NetworkInformation.Ping p in pingers)
            {
                p.PingCompleted -= Ping_completed;
                p.Dispose();
            }

            pingers.Clear();

        }


            */
        }
    }

}

