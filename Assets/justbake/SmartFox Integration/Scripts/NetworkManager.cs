
using justbake.sfi.Assets.justbake.SmartFox_Integration.Scripts;
using PlasticGui.Configuration.CloudEdition.Welcome;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Logging;
using Sfs2X.Requests;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor.SearchService;
using UnityEngine;

namespace justbake.sfi
{
	[DisallowMultipleComponent, RequireComponent(typeof(NetworkLogin))]
	[AddComponentMenu("Network/Network Manager")]
	public class NetworkManager : MonoBehaviour {

		public static NetworkManager Instance;

		#region Editor
		[Header("Configuration")]
		[SerializeField] bool DestroyOnLoad = false;
		[SerializeField] bool RunInBackground = false;

		[Header("Network Info")]
		[SerializeField] internal ConnectionSettings ConnectionSettings;
		[SerializeField] private bool ConnectOnStart;

		[SerializeField] internal bool EnableLagMonitor = true;
		[SerializeField] internal int Interval = 4;
		[SerializeField] internal int QueueSize = 10;

		[Header("Login")]
		public NetworkLogin networkLogin;

		[Header("Scene Management")]
		[SerializeField] internal string OfflineScene;
		[SerializeField] internal string OnilneScene;

		[Header("Player Object")]
		[SerializeField] internal GameObject PlayerPrefab;
		[SerializeField] internal GameObject SpectatorPrefab;
		[Tooltip("Will Spawn the player when the user is logged in.")]
		[SerializeField] internal bool AutoCreatePlayer;
		[Tooltip("Will Spawn the player when the user joins a room.")]
		[SerializeField] internal bool CreatePlayerForEachRoom;
		[Tooltip("Will destroy the player when the user leaves a room.")]
		[SerializeField] internal bool DestroyPlayerForEachRoom;

		[Header("Debug Settings")]
		[SerializeField] internal bool EnableConsoleTrace = false;
		[SerializeField] LogLevel LogLevel = LogLevel.ERROR;

		public List<GameObject> SpawnablePrefabs = new List<GameObject>();
		#endregion

		#region public properties

		public User LocalUser => (sfs != null) ? sfs.MySelf : null;
		public bool IsConnected => sfs != null && sfs.IsConnected;
		public bool IsConnecting => sfs != null && sfs.IsConnecting;
		public bool IsLoggedIn => sfs != null && LocalUser != null;
		public bool IsUdpAvailable => sfs != null && sfs.UdpAvailable;

		/// <summary>
		///  The average of the last ten measured lag values, expressed in milliseconds.
		/// </summary>
		public int LagValue => lagValue;
		#endregion

		#region private properties

		internal SmartFox sfs;

		private int lagValue;

		private GameObject LocalUserGameObject;

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
		}

		public void InitUdp() {
			sfs.InitUDP();
		}

		public void JoinRoom(object id, string password = null, int? roomIdToLeave = null, bool asSpectator = false) {
			if (!IsLoggedIn || id == null) {
				return;
			}

			sfs.Send(new JoinRoomRequest(id, password, roomIdToLeave, asSpectator));
		}

		public void LeaveRoom(object id) {
			if (sfs.TryGetRoomUserIsIn(id, out Room room)) {
				sfs.Send(new LeaveRoomRequest(room));
			} else {
				Debug.LogError($"User not joined in room with {((id is string) ? "name" : "id")} {id}");
			}
		}
		#endregion
		#region protected
		#region virtual
		protected virtual void OnConnectionSuccess() {}
		protected virtual void OnConnectionFailure() {}
		protected virtual void OnConnectionLost(string reason) {}
		protected virtual void OnRoomJoin(Room room) {}
		protected virtual void OnRoomJoinError(string message, short code) {}

		/*internal virtual void SpawnPlayer(User user) {
			if(user.Equals(LocalUser) && LocalUserGameObject != null && !CreatePlayerForEachRoom) {
				return;
			}
			if((!user.IsSpectator && PlayerPrefab != null) || (user.IsSpectator && SpectatorPrefab != null)) {
				GameObject player = user.IsSpectator ? Instantiate(SpectatorPrefab) : Instantiate(PlayerPrefab);
				NetworkUser playerUser = player.GetComponent<NetworkUser>();
				playerUser.user = user;
				player.name = user.Name + "_" + user.Id;
				if (user.Equals(LocalUser)) {
					LocalUserGameObject = player;
				}
			}
		}*/

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

			if (EnableLagMonitor)
				sfs.AddEventListener(SFSEvent.PING_PONG, OnPingPong);
#endif

			sfs.AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
			sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);

			sfs.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
			sfs.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserExitRoom);
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
				networkLogin = GetComponent<NetworkLogin>();
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

		public virtual void OnValidate() {
			if (PlayerPrefab != null && PlayerPrefab.GetComponent<NetworkUser>() == null) {
				Debug.LogError("NetworkManager - PlayerPrefab must have a NetworkUser.");
				PlayerPrefab = null;
			}

			// This avoids the mysterious "Replacing existing prefab with assetId ... Old prefab 'Player', New prefab 'Player'" warning.
			if (PlayerPrefab != null && SpawnablePrefabs.Contains(PlayerPrefab)) {
				Debug.LogWarning("NetworkManager - Player Prefab should not be added to Registered Spawnable Prefabs list...removed it.");
				SpawnablePrefabs.Remove(PlayerPrefab);
			}
		}
		#endregion

		#region Smartfox Callbacks
		#region Connection
		/// <summary>
		/// Dispatched when a connection between the client and a SmartFoxServer 2X instance is attempted.
		/// </summary>
		/// <param name="evt">
		/// success	(bool) The connection result: true if a connection was established, false otherwise.
		/// </param>
		private void OnConnection(BaseEvent evt) {
			if (evt.TryGetSuccessParam(out bool success) && success) {
				Debug.Log("Connection established successfully");
				Debug.Log($"SFS2X API version: {sfs.Version}");
				Debug.Log($"Connection mode is: {sfs.ConnectionMode}");
				OnConnectionSuccess();
				networkLogin.OnStartConnection();
#if !UNITY_WEBGL
				if (ConnectionSettings.Encrypt)
					sfs.InitCrypto();
				else if (networkLogin.LoginOnConnection) {
					networkLogin.Login();
				}
#else
				if (LoginOnConnection)
					Login(username, password, parameters);
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
			if (evt.TryGetReasonMessageParam(out string reason)) {
				Debug.Log($"Connection was lost, Reason: {reason}");
				OnConnectionLost(reason);
			}
			networkLogin.OnStopConnection();
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
			if (evt.TryGetSuccessParam(out bool success) && success) {
				if (networkLogin.LoginOnConnection) {
					networkLogin.Login();
				}
			}else if (evt.TryGetErrorMessageParam(out string errorMessage)) {
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
			if (evt.TryGetSuccessParam(out bool success) && success) {
				Debug.Log("UDP Ready!");
			} else {
				Debug.Log("UDP Error!");
			}
		}

		/// <summary>
		/// Dispatched when a new lag value measurement is available.
		/// </summary>
		/// <param name="evt">
		/// lagValue	(int) The average of the last ten measured lag values, expressed in milliseconds.
		/// </param>
		private void OnPingPong(BaseEvent evt) {
			if (evt.TryGetLagValueParam(out int lagValue)) {
				this.lagValue = lagValue;
			}
		}
		#endregion
		#region Rooms
		/// <summary>
		/// Dispatched when a Room is joined by the current user.
		/// </summary>
		/// <param name="evt">
		/// room	(Room) An object representing the Room that was joined.
		/// </param>
		private void OnRoomJoin(BaseEvent evt) {
			if (evt.TryGetRoomParam(out Room room)) {
				Debug.Log("Room joined: " + room.Name);
				foreach (User user in room.UserList) {
					//SpawnPlayer(user);
				}
				OnRoomJoin(room);
				//TODO: make the scene manager load the the appropriate scene the represents the room
			}
		}

		/// <summary>
		/// Dispatched when an error occurs while the current user is trying to join a Room.
		/// </summary>
		/// <param name="evt">
		/// errorMessage	(string) A message containing the description of the error.
		///errorCode	(short) The error code.
		/// </param>
		private void OnRoomJoinError(BaseEvent evt) {
			if (evt.TryGetErrorMessageParam(out string errorMessage) && evt.TryGetErrorCodeParam(out short errorCode)) {
				Debug.LogError($"Room Join Error({errorCode}):\n{errorMessage}");
				OnRoomJoinError(errorMessage, errorCode);
			}
		}
		/// <summary>
		/// Dispatched when one of the Rooms joined by the current user is entered by another user.
		/// </summary>
		/// <param name="evt">
		/// user	(User) An object representing the user who joined the Room.
		/// room	(Room) An object representing the Room that was joined by a user.
		/// </param>
		private void OnUserEnterRoom(BaseEvent evt) {
			if(evt.TryGetUserParam(out User user) && evt.TryGetRoomParam(out Room room)) {
				//SpawnPlayer(user);
				Debug.Log($"User: {user.Name} has just joined room: {room.Name}");
			}
		}
		/// <summary>
		/// Dispatched when one of the Rooms joined by the current user is left by another user, or by the current user himself.
		/// </summary>
		/// <param name="evt">
		/// user	(User) An object representing the user who left the Room.
		/// room	(Room) An object representing the Room that was left by the user.
		/// </param>
		private void OnUserExitRoom(BaseEvent evt) {
			//TODO: destroy user game object in scene if the room is the current scene.
			//TODO: if the local user left make the scene manager unload the appropriate scene.
			if (evt.TryGetUserParam(out User user) && evt.TryGetRoomParam(out Room room)) {
				if (user.Equals(LocalUser)) {
					foreach (User userToDestroy in room.UserList) {
						Destroy(GameObject.Find(userToDestroy.Name + "_" + userToDestroy.Id));
					}
					if(DestroyPlayerForEachRoom)
						Destroy(GameObject.Find(user.Name + "_" + user.Id));
				} else {
					Destroy(GameObject.Find(user.Name + "_" + user.Id));
				}
				Debug.Log($"User: {user.Name} has just left room: {room.Name}");
			}
		}
		#endregion
		#endregion
	}
}
