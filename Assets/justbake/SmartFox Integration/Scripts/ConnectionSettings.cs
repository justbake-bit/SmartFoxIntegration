using Sfs2X.Util;
using System;
using System.Collections;
using UnityEngine;

namespace justbake.sfi.Assets.justbake.SmartFox_Integration.Scripts {
	[CreateAssetMenu(menuName ="Network/Connection Settings")]
	[Serializable]
	public class ConnectionSettings : ScriptableObject {

		[Header("Connection Settings")]
		[Tooltip("Indicates whether the client-server messages debug should be enabled or not.")]
		public bool Debug = false;
		[Tooltip("Indicates whether the encryption of messages exchanged between SmartFoxServer and its client requires an initial HTTPS negotiation which allows the client to acquire the private key later used for 128 bit symmetric encryption of the communication.")]
		public bool Encrypt = false;
		[Tooltip("Specifies the IP address or host name of the SmartFoxServer 2X instance to connect to (TCP connection).")]
        public string Host = "localhost";
        [Tooltip("Specifies the port for generic HTTP communication.")]
        public int HttpPort = 8080;
        [Tooltip("Specifies the port for HTTPS communication.")]
        public int HttpsPort = 8080;
        [Tooltip("Specifies the TCP port of the SmartFoxServer 2X instance to connect to (TCP connection).")]
        public int Port = 9933;
        [Tooltip("Indicates whether SmartFoxServer's TCP socket is using the Nagle algorithm or not.")]
        public bool TcpNoDelay;
        [Tooltip("Specifies the IP address of the SmartFoxServer 2X instance to connect to (UDP connection).")]
        public string UdpHost = "localhost";
        [Tooltip("Specifies the UDP port of the SmartFoxServer 2X instance to connect to (UDP connection).")]
        public int UdpPort = 9933;
        [Tooltip("Specifies the Zone of the SmartFoxServer 2X instance to join.")]
        public string Zone = "BasicExamples";

		[Header("Bluebox settings")]
		[Tooltip("Indicates whether the SmartFoxServer's BlueBox should be enabled or not.")]
		public bool IsActive = true;
		[Tooltip("Specifies the BlueBox polling speed.")]
		public int PollingRate = 500;

		[Header("Proxy server settings for the BlueBox.")]
		[Tooltip("Indicates whether the proxy server should be bypassed for local addresses or not.")]
		public bool ByPassLocal = false;
        [Tooltip("Specifies the IP address of the proxy server.")]
        public string ProxyHost = "localhost";
		[Tooltip("Password for the proxy server authentication.")]
		public string Password = "";
		[Tooltip("Specifies the port number of the proxy server.")]
		public int ProxtPort = 9933;
        [Tooltip("User name for the proxy server authentication.")]
        public string UserName = "";
        [Tooltip("Forces the BlueBox connection to use an HTTPS tunnel instead of standard HTTP.")]
		public bool UseHttps = false;

		public ConfigData GetConfig() {
			ConfigData cfg = new ConfigData();

			cfg.Host = Host;
			cfg.HttpPort = HttpPort;
			cfg.HttpsPort = HttpsPort;
			cfg.Port = Port;
#if UNITY_WEBGL
			cfg.Port = Encrypt ? httpsPort : httpPort;
#endif
			cfg.TcpNoDelay = TcpNoDelay;
			cfg.UdpHost = UdpHost;
			cfg.UdpPort = UdpPort;
			cfg.Zone = Zone;

            cfg.BlueBox.UseHttps = UseHttps;
            cfg.BlueBox.IsActive = IsActive;
            cfg.BlueBox.PollingRate = PollingRate;

			ProxyCfg proxyCfg = new ProxyCfg();
			proxyCfg.BypassLocal = ByPassLocal;
			proxyCfg.Host = ProxyHost;
            proxyCfg.Port = Port;
            proxyCfg.Password = Password;
			proxyCfg.UserName = UserName;

            cfg.BlueBox.Proxy = proxyCfg;


			return cfg;
		}
	}
}