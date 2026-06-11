using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;


namespace MMXOnline;

public class LANIPHelper
{
	private object lockObj = new object();

	private HashSet<string> ips = new HashSet<string>();

	private string ipBase;

	public LANIPHelper()
	{
		List<string> pieces = GetLocalIPAddress().Split('.').ToList();
		pieces.Pop();
		ipBase = string.Join(".", pieces);
		ipBase += ".";
	}

	public static string GetLocalIPAddress()
	{
		IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
		foreach (IPAddress ip in addressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				return ip.ToString();
			}
		}
		throw new Exception("Failed to get local LAN IP address.");
	}

	public bool isLANIP(string ip)
	{
		if (!ip.StartsWith(ipBase))
		{
			return ip == "127.0.0.1";
		}
		return true;
	}

	public List<string> getIps()
	{
		List<string> ips = new List<string>();
		for (int i = 1; i < 20; i++)
		{
			string ip = ipBase + i;
			if (ip.IsValidIpAddress())
			{
				ips.Add(ip);
			}
		}
		return ips;
	}
}

/*
namespace MMXOnline;

public class LANIPHelper {
	object lockObj = new object();
	HashSet<string> ips = new HashSet<string>();
	string ipBase;

	public LANIPHelper() {
		string localIp = GetLocalIPAddress();
		var pieces = localIp.Split('.').ToList();
		pieces.Pop();
		ipBase = string.Join(".", pieces);
		ipBase += ".";
	}

	public static string GetLocalIPAddress() {
		string? localIP;
		using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
			socket.Connect("8.8.8.8", 0);
			IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
			localIP = endPoint?.Address.ToString();
			socket.Disconnect(true);
			socket.Dispose();
		}
		return localIP ?? "127.0.0.1";
	}

	public bool isLANIP(string ip) {
		return ip.StartsWith(ipBase) || ip == "127.0.0.1";
	}

	public List<string> getIps() {
		var ips = new List<string>();
		// Really really slow for anywhere near 200. Hence we only look for the first 20 ip's found on LAN.
		// Could investigate why more to make LAN ip lookup more automated
		for (int i = 1; i < 20; i++) {
			string ip = ipBase + i.ToString();
			if (ip.IsValidIpAddress()) {
				ips.Add(ip);
			}
		}
		return ips;
	}
}
*/