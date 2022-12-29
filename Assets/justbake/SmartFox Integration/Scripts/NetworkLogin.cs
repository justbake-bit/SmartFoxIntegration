using justbake.sfi.Assets.justbake.SmartFox_Integration.Scripts;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms.Impl;

namespace justbake.sfi {
	[DisallowMultipleComponent]
	public class NetworkLogin : MonoBehaviour {

		[Header("Login Settings")]
		[SerializeField] internal bool DisconnectOnLogout;
		[SerializeField] internal bool LoginOnConnection;
		[SerializeField] internal bool InitUdpOnLogin;

		/// <summary>Notify subscribers on the client when the client is authenticated</summary>
		public UnityEvent OnClientAuthenticated = new UnityEvent();

		public void Login() {
			NetworkManager.Instance.sfs.Send(new LoginRequest(GetUsername(), GetPassword(), NetworkManager.Instance.ConnectionSettings.Zone, GetData()));
		}

		public void Logout() {
			NetworkManager.Instance.sfs.Send(new LogoutRequest());
		}

		protected virtual string GetUsername() {
			return "";
		}

		protected virtual string GetPassword() {
			return "";
		}

		protected virtual ISFSObject GetData() {
			return new SFSObject();
		}

		/// <summary>Called when connection starts, used to register message handlers if needed.</summary>
		internal virtual void OnStartConnection() {
			AddEventListeners();
		}

		/// <summary>Called when connection stops, used to unregister message handlers if needed.</summary>
		internal virtual void OnStopConnection() {
		}

		protected virtual void OnLoginSuccess(ISFSObject data) {
		}
		protected virtual void OnLoginError(string message, short code) {
		}
		protected virtual void OnLogout() {
		}

		/// <summary>
		/// Dispatched when the current user performs a successful login in a server Zone.
		/// </summary>
		/// <param name="evt">
		/// user	(User) An object representing the user who performed the login.
		/// data	(ISFSObject) An object containing custom parameters returned by a custom login system, if any.
		/// </param>
		private void OnLogin(BaseEvent evt) {
			if (evt.TryGetUserParam(out User user)) {
				NetworkManager.Instance.sfs.EnableLagMonitor(NetworkManager.Instance.EnableLagMonitor, NetworkManager.Instance.Interval, NetworkManager.Instance.QueueSize);
				if (NetworkManager.Instance.AutoCreatePlayer) {
					//NetworkManager.Instance.SpawnPlayer(NetworkManager.Instance.LocalUser);
				}
				Debug.Log($"You have logged in as {user.Name}");
				OnClientAuthenticated?.Invoke();
			} else {
				Debug.LogError("The param user was not found or is not a user!");
			}

			if (!evt.TryGetDataParam(out ISFSObject data)) {
				data = new SFSObject();
			}

			OnLoginSuccess(data);
			if (InitUdpOnLogin)
				NetworkManager.Instance.InitUdp();
		}
		/// <summary>
		/// Dispatched if an error occurs while the user login is being performed.
		/// </summary>
		/// <param name="evt">
		/// errorMessage	(string) A message containing the description of the error.
		/// errorCode	(short) The error code.
		/// </param>
		private void OnLoginError(BaseEvent evt) {
			if (evt.TryGetErrorMessageParam(out string errorMessage) && evt.TryGetErrorCodeParam(out short errorCode)) {
				Debug.LogError($"Login Error {errorCode}:\n{errorMessage}");
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
				NetworkManager.Instance.Disconnect();
			}
		}

		private void AddEventListeners() {
			NetworkManager.Instance.sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);
			NetworkManager.Instance.sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
			NetworkManager.Instance.sfs.AddEventListener(SFSEvent.LOGOUT, OnLogout);
		}
	}
}