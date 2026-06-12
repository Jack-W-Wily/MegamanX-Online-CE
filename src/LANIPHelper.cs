using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;


namespace MMXOnline;

public class LANIPHelper {
	public object lockObj = new object();
	public HashSet<string> ips = new HashSet<string>();
	public string ipBase;

	public LANIPHelper() {
		List<string> pieces = GetLanIPAddress().Split('.').ToList();
		pieces.Pop();
		ipBase = string.Join(".", pieces);
		ipBase += ".";
	}

	public static string GetLocalIPAddress() {
		string? localIP;
		try {
			using WebClient wc = new();
			localIP = wc.DownloadString("https://api.ipify.org/");
		} catch {
			return "127.0.0.1";
		}
		return localIP ?? "127.0.0.1";
	}

	public static string GetLanIPAddress() {
		IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
		foreach (IPAddress ip in addressList) {
			if (ip.AddressFamily == AddressFamily.InterNetwork) {
				return ip.ToString();
			}
		}
		return "127.0.0.1";
	}

	public bool isLANIP(string ip) {
		return ip.StartsWith(ipBase) || ip == "127.0.0.1";
	}

	public List<string> getIps() {
		var ips = new List<string>();
		// Really really slow for anywhere near 200.
		// Hence we only look for the first 20 ip's found on LAN.
		// Could investigate why more to make LAN ip lookup more automated.
		for (int i = 1; i < 20; i++) {
			string ip = ipBase + i;
			if (ip.IsValidIpAddress()) {
				ips.Add(ip);
			}
		}
		return ips;
	}
}
