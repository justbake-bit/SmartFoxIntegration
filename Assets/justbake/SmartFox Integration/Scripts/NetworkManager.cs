
using justbake.sfi.Assets.justbake.SmartFox_Integration.Scripts;
using PlasticGui.Configuration.CloudEdition.Welcome;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Logging;
using Sfs2X.Requests;
using UnityEngine;

namespace justbake.sfi
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Network/Network Manager")]
	public class NetworkManager : MonoBehaviour {

		public static NetworkManager Instance;

		#region Editor
		[Header("Singleton Settings")]
		[SerializeField] bool DestroyOnLoad = false;
		[SerializeField] bool RunInBackground = false;
		[Header("Connection Settings")]
		[SerializeField] private ConnectionSettings ConnectionSettings;
		[SerializeField] private bool ConnectOnStart;

		[Header("Login Settings")]
		[SerializeField] private bool DisconnectOnLogout;
		[SerializeField] private bool LoginOnConnection;
		[SerializeField] private bool InitUdpOnLogin;
		[Header("Login Details")]
		public string username = null;
		public string password = null;
		public string zoneName = null;
		public ISFSObject parameters = null;

		[Header("Smartfox settings")]
		[SerializeField] bool EnableLagMonitor = true;
		[SerializeField] int Interval = 4;
		[SerializeField] int QueueSize = 10;

		[Header("Debug Settings")]
		[SerializeField] bool EnableConsoleTrace = false;
		[SerializeField] LogLevel LogLevel = LogLevel.ERROR;
		#endregion

		#region public properties

		public User LocalUser => (sfs != null) ? sfs.MySelf : null;
		public bool IsConnected => sfs != null && sfs.IsConnected;
		public bool IsConnecting => sfs != null && sfs.IsConnecting;
		public bool IsLogedIn => sfs != null && LocalUser != null;
		public bool IsUdpAvailable => sfs != null && sfs.UdpAvailable;

		/// <summary>
		///  The average of the last ten measured lag values, expressed in milliseconds.
		/// </summary>
		public int LagValue => lagValue;
		#endregion

		#region private properties

		private SmartFox sfs;

		private int lagValue;

		#endregion

		#region Methods
		#region public
		public void Connect() {
			if (IsConnecting || IsConnected)
				return;

			sfs = new SmartFox();

			sfs.Logger.EnableConsoleTrace = EnableConsoleTrace;
			sfs.Logger.LoggingLevel = LogLevel;

			AddDefaultEventListeners();

#if !UNITY_WEBGL
			sfs.Connect(ConnectionSettings.GetConfig());
#else
			sfs = new SmartFox(ConnectionSettings.Encrypt ? UseWebSocket.WSS_BIN : UseWebSocket.WS_BIN);
#endif
		}

		public void Disconnect() {
			sfs.Disconnect();
			sfs.RemoveAllEventListeners();
			sfs = null;
		}

		public void Login(string username = null, string password = null, string zoneName = null, ISFSObject parameters = null) {
			if (username != null && password != null && zoneName != null && parameters != null) {
				sfs.Send(new LoginRequest(username, password, zoneName, parameters));
			} else if (username != null && password != null && zoneName != null && parameters == null) {
				sfs.Send(new LoginRequest(username, password, zoneName));
			} else if (username != null && password != null && zoneName == null && parameters == null) {
				sfs.Send(new LoginRequest(username, password));
			} else if (username != null && password == null && zoneName == null && parameters == null) {
				sfs.Send(new LoginRequest(username));
			} else if (username == null && password == null && zoneName == null && parameters == null) {
				sfs.Send(new LoginRequest(""));
			}
		}

		public void Logout() {
		
		}

		public void InitUdp() {
			
		}
		#endregion
		#region protected
		#region virtual
		protected virtual void OnConnectionSuccess() {}
		protected virtual void OnConnectionFailure() {}
		protected virtual void OnConnectionLost(string reason) {}
		protected virtual void OnLoginSuccess(ISFSObject data) {}
		protected virtual void OnLoginError(string message, short code) {}
		protected virtual void OnLogout() {}
		#endregion
		#endregion
		#region private
		private void AddDefaultEventListeners() {
			sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);
			sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);

			sfs.AddEventListener(SFSEvent.UDP_INIT, OnUdpInit);

#if !UNITY_WEBGL
			if (ConnectionSettings.Encrypt)
				sfs.AddEventListener(SFSEvent.CRYPTO_INIT, OnCryptoInit);
#endif

			sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);
			sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
			sfs.AddEventListener(SFSEvent.LOGOUT, OnLogout);

			if (EnableLagMonitor)
				sfs.AddEventListener(SFSEvent.PING_PONG, OnPingPong);
		}
		#endregion
		#endregion

		#region Monobehaviour
		public virtual void Awake() {
			if (Instance != null && Instance != this) {
				Destroy(this);
			} else {
				Instance = this;
				if(!DestroyOnLoad)
					DontDestroyOnLoad(this);
				Application.runInBackground = RunInBackground;
			}
		}
		public virtual void Start() {
			if(ConnectOnStart) Connect();
		}
		public virtual void Update() {
			if(sfs != null)
				sfs.ProcessEvents();
		}

		public virtual void OnApplicationQuit() {
			if(sfs != null && sfs.IsConnected)
				sfs.Disconnect();
		}
		#endregion

		#region Smartfox Callbacks
		/// <summary>
		/// Dispatched when a connection between the client and a SmartFoxServer 2X instance is attempted.
		/// </summary>
		/// <param name="evt">
		/// success	(bool) The connection result: true if a connection was established, false otherwise.
		/// </param>
		private void OnConnection(BaseEvent evt) {
			if (evt.Params.TryGetValue("success", out object value) && value is bool success && success) {
				Debug.Log("Connection established successfully");
				Debug.Log($"SFS2X API version: {sfs.Version}");
				Debug.Log($"Connection mode is: {sfs.ConnectionMode}");
				OnConnectionSuccess();
#if !UNITY_WEBGL
				if (ConnectionSettings.Encrypt)
					sfs.InitCrypto();
				else if (LoginOnConnection)
					Login(username, password, zoneName, parameters);
#else
				if (LoginOnConnection)
					Login(username, password, zoneName, parameters);
#endif

			} else {
				OnConnectionFailure();
			}
		}

		/// <summary>
		/// Dispatched when the connection between the client and the SmartFoxServer 2X instance is interrupted.
		/// </summary>
		/// <param name="evt">
		///	reason	(string) The reason of the disconnection, among those available in the ClientDisconnectionReason class.
		/// </param>>
		private void OnConnectionLost(BaseEvent evt) {
			if (sfs != null) {
				sfs.RemoveAllEventListeners();
				sfs = null;
			}
			if (evt.Params.TryGetValue("reason", out object value) && value is string reason) {
				Debug.Log($"Connection was lost, Reason: {reason}");
				OnConnectionLost(reason);
			}
		}
#if !UNITY_WEBGL
		/// <summary>
		/// Dispatched in return to the initialization of an encrypted connection.
		/// </summary>
		/// <param name="evt">
		/// success	(bool) true if a unique encryption key was successfully retrieved via HTTPS, false if the transaction failed.
		/// errorMessage	(string) If success is false, provides additional details on the occurred error.
		/// </param>
		private void OnCryptoInit(BaseEvent evt) {
			if (evt.Params.TryGetValue("success", out object value) && value is bool success && success) {
				if (LoginOnConnection)
					Login(username, password, zoneName, parameters);
			}else if (evt.Params.TryGetValue("errorMessage", out object value1) && value1 is string errorMessage) {
				Debug.Log($"Encryption initialization failed: {errorMessage}");
				sfs.Disconnect();
			}
		}
#endif
		/// <summary>
		/// Dispatched when the result of the UDP handshake is notified.
		/// </summary>
		/// <param name="evt">
		/// success	(bool) true if UDP connection initialization is successful, false otherwise.
		/// </param>
		private void OnUdpInit(BaseEvent evt) {
			if (evt.Params.TryGetValue("success", out object value) && value is bool success && success) {
				Debug.Log("UDP Ready!");
			} else {
				Debug.Log("UDP Error!");
			}
		}
		/// <summary>
		/// Dispatched when the current user performs a successful login in a server Zone.
		/// </summary>
		/// <param name="evt">
		/// user	(User) An object representing the user who performed the login.
		/// data	(ISFSObject) An object containing custom parameters returned by a custom login system, if any.
		/// </param>
		private void OnLogin(BaseEvent evt) {
			if (evt.Params.TryGetValue("user", out object value) && value is User user) {
				sfs.EnableLagMonitor(EnableLagMonitor, Interval, QueueSize);
				if (LocalUser != user)
					Debug.LogError($"The user that logged {user} in is not the localUser {LocalUser}");
			} else {
				Debug.LogError("The param user was not found or is not a user!");
			}
			ISFSObject data = new SFSObject();

			if (evt.Params.TryGetValue("data", out object objData) && objData is ISFSObject isfsdata) {
					data = isfsdata;
			}

			OnLoginSuccess(data);
			if (InitUdpOnLogin)
				InitUdp();
		}
		/// <summary>
		/// Dispatched if an error occurs while the user login is being performed.
		/// </summary>
		/// <param name="evt">
		/// errorMessage	(string) A message containing the description of the error.
		/// errorCode	(short) The error code.
		/// </param>
		private void OnLoginError(BaseEvent evt) {
			if (evt.Params.TryGetValue("errorMessage", out object value) && value is string errorMessage && evt.Params.TryGetValue("errorCode", out object value1) && value1 is short errorCode) {
				Debug.LogError($"Login Error({errorCode}):\n{errorMessage}");
				OnLoginError(errorMessage, errorCode);
			}
		}
		/// <summary>
		/// Dispatched when the current user performs logs out of the server Zone.
		/// </summary>
		/// <param name="evt">
		/// 
		/// </param>
		private void OnLogout(BaseEvent evt) {
			OnLogout();

			if (DisconnectOnLogout) {
				Disconnect();
			}
		}

		/// <summary>
		/// Dispatched when a new lag value measurement is available.
		/// </summary>
		/// <param name="evt">
		/// lagValue	(int) The average of the last ten measured lag values, expressed in milliseconds.
		/// </param>
		private void OnPingPong(BaseEvent evt) {
			if (evt.Params.TryGetValue("lagValue", out object value) && value is int lagValue) {
				this.lagValue = lagValue;
			}
		}
		#endregion
	}
}
