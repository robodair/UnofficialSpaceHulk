using UnityEngine;
using System.Collections;
using System.Net.NetworkInformation;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
//Created by Stephen on 13-8-14
public class NetMod : MonoBehaviour {


	static void Main(string[] args)
	{

	}


	public static void Connect()
	{

		//Starts the C# server to send string data to
		Process.Start("Assets\\Code\\Server");
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
		IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 137);
		
		Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		
		
		try
		{
			
			server.Connect(ipep);
		}
		catch (SocketException e)
		{
			UnityEngine.Debug.Log("Unable to connect to the server.");
			UnityEngine.Debug.Log(e.ToString());
			
			return;
		}
		
		int recv = server.Receive(data);
		stringData = Encoding.ASCII.GetString(data, 0, recv);
		UnityEngine.Debug.Log(stringData);
		int i = 0;
		while (true)
		{ 
			 i = i + 1;
			input = "Client Side: " + i;
			server.Send(Encoding.ASCII.GetBytes(input));
			data = new byte[1024];
			try
			{
				recv = server.Receive(data);
				
				stringData = Encoding.ASCII.GetString(data, 0, recv);
				UnityEngine.Debug.Log(ipep.Address + ": " + stringData);
				if (i == 5)
				{
					
					string msg = "Closing connection";
					server.Send(Encoding.ASCII.GetBytes(msg));
					break;
				}

			}
			catch
			{
				UnityEngine.Debug.Log("Server disconnected.");
				break;
			}
			
		}
		UnityEngine.Debug.Log("Disconnecting from server");
		server.Shutdown(SocketShutdown.Both);
		server.Close();

		}



	public static void Server()
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



			listenPort = 137;

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
			UnityEngine.Debug.Log("An error occured on that port. Using port 137 as default.");
			ipep = new IPEndPoint(IPAddress.Any, 137);
		}
		
		
		
		Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		
		newsock.Bind(ipep);
		newsock.Listen(10);
		UnityEngine.Debug.Log("Waiting for client....");

		Socket client = newsock.Accept();
		IPEndPoint clientep = (IPEndPoint)client.RemoteEndPoint;
		
		UnityEngine.Debug.Log("Connected with  at port "+ clientep.Address+ clientep.Port);
		
		string Welcome = "YOU ARE CONNECTED";
		string info = "Connected with  at port "+ clientep.Address+ clientep.Port;
		data = Encoding.ASCII.GetBytes(Welcome);
		client.Send(data, data.Length, SocketFlags.None);
		data = Encoding.ASCII.GetBytes (info);
		client.Send(data, data.Length, SocketFlags.None);
		int i = 0;
		while (true)
		{ i = i+1;
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
			
			
			UnityEngine.Debug.Log(clientep.Address+": "+Encoding.ASCII.GetString(data, 0, recv));
			UnityEngine.Debug.Log(localIP + ": ");
			
			msg = new byte[1024];
			stMsg = "Server Side: " + i;
			msg = Encoding.ASCII.GetBytes(stMsg);
			if (i == 5)
			{

				msg.Equals( "Closing connection.");
			client.Send(msg, msg.Length, SocketFlags.None);
				break;
			}
			else
			{

				client.Send(msg, msg.Length, SocketFlags.None);
			}
		}
		
		UnityEngine.Debug.Log("Disconnected from client " + clientep.Address);
		
		client.Close();
		newsock.Close();
		
	}




}