using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sfs2X.Entities;
using System;
using System.Globalization;

namespace justbake.sfi
{
	public class NetworkUser : MonoBehaviour {
		/// <summary>
		/// Keep track of all sceneIds to detect scene duplicates
		/// </summary>
		static readonly Dictionary<ulong, NetworkUser> sceneIds = new Dictionary<ulong, NetworkUser>();

		/// <summary>
		/// to save bandwidth, we send one 64 bit dirty mask
		/// instead of 1 byte index per dirty component.
		/// which means we can't allow > 64 components (it's enough).
		/// </summary>
		const int MaxNetworkBehaviours = 64;

		public User user {
			get; internal set;
		}

		public int Id => user.Id;

		public string Name => user.Name;

		public bool IsLocalUser => NetworkManager.Instance.LocalUser.Equals(user);

		/// <summary>isOwned is true on the client if this NetworkIdentity is one of the .owned entities of our connection on the server.</summary>
		// for example: main player & pets are owned. monsters & npcs aren't.
		public bool IsOwned {
			get; internal set;
		}

		// hasSpawned should always be false before runtime
		[SerializeField, HideInInspector] bool hasSpawned;
		public bool SpawnedFromInstantiate {
			get; private set;
		}

		/// <summary>
		/// all NetworkBehaviour components
		/// </summary>
		public NetworkUserBehaviour[] NetworkBehaviours {
			get; private set;
		}

		public virtual void Awake() {
			InitializeNetworkUserBehaviours();

			if (hasSpawned) {
				Debug.LogError($"{name} has already spawned. Don't call Instantiate for NetworkIdentities that were in the scene since the beginning (aka scene objects).  Otherwise the client won't know which object to use for a SpawnSceneObject message.");
				SpawnedFromInstantiate = true;
				Destroy(gameObject);
			}
			hasSpawned = true;
		}

		private void OnValidate() {
			hasSpawned = false;
		}

		internal void InitializeNetworkUserBehaviours() {
			// Get all NetworkBehaviours
			// (never null. GetComponents returns [] if none found)
			NetworkBehaviours = GetComponents<NetworkUserBehaviour>();
			ValidateComponents();

			// initialize each one
			for (int i = 0; i < NetworkBehaviours.Length; ++i) {
				NetworkUserBehaviour component = NetworkBehaviours[i];
				component.netUserIdentity = this;
				component.ComponentIndex = (byte) i;
			}
		}

		bool clientStarted;
		internal void OnStartClient() {
			if (clientStarted)
				return;

			clientStarted = true;

			// Debug.Log($"OnStartClient {gameObject} netId:{netId}");
			foreach (NetworkUserBehaviour comp in NetworkBehaviours) {
				// an exception in OnStartClient should be caught, so that one
				// component's exception doesn't stop all other components from
				// being initialized
				// => this is what Unity does for Start() etc. too.
				//    one exception doesn't stop all the other Start() calls!
				try {
					// user implemented startup
					comp.OnStartClient();
				} catch (Exception e) {
					Debug.LogException(e, comp);
				}
			}
		}

		internal void OnStopClient() {
			// In case this object was destroyed already don't call
			// OnStopClient if OnStartClient hasn't been called.
			if (!clientStarted)
				return;

			foreach (NetworkUserBehaviour comp in NetworkBehaviours) {
				// an exception in OnStopClient should be caught, so that
				// one component's exception doesn't stop all other components
				// from being initialized
				// => this is what Unity does for Start() etc. too.
				//    one exception doesn't stop all the other Start() calls!
				try {
					comp.OnStopClient();
				} catch (Exception e) {
					Debug.LogException(e, comp);
				}
			}
		}

		bool hadAuthority;
		internal void NotifyAuthority() {
			if (!hadAuthority && IsOwned)
				OnStartAuthority();
			if (hadAuthority && !IsOwned)
				OnStopAuthority();
			hadAuthority = IsOwned;
		}

		internal void OnStartAuthority() {
			foreach (NetworkUserBehaviour comp in NetworkBehaviours) {
				// an exception in OnStartAuthority should be caught, so that one
				// component's exception doesn't stop all other components from
				// being initialized
				// => this is what Unity does for Start() etc. too.
				//    one exception doesn't stop all the other Start() calls!
				try {
					comp.OnStartAuthority();
				} catch (Exception e) {
					Debug.LogException(e, comp);
				}
			}
		}

		internal void OnStopAuthority() {
			foreach (NetworkUserBehaviour comp in NetworkBehaviours) {
				// an exception in OnStopAuthority should be caught, so that one
				// component's exception doesn't stop all other components from
				// being initialized
				// => this is what Unity does for Start() etc. too.
				//    one exception doesn't stop all the other Start() calls!
				try {
					comp.OnStopAuthority();
				} catch (Exception e) {
					Debug.LogException(e, comp);
				}
			}
		}

		private void ValidateComponents() {
			if (NetworkBehaviours == null) {
				Debug.LogError($"NetworkBehaviours array is null on {gameObject.name}!\n" +
					$"Typically this can happen when a networked object is a child of a " +
					$"non-networked parent that's disabled, preventing Awake on the networked object " +
					$"from being invoked, where the NetworkBehaviours array is initialized.", gameObject);
			} else if (NetworkBehaviours.Length > MaxNetworkBehaviours) {
				Debug.LogError($"NetworkIdentity {name} has too many NetworkBehaviour components: only {MaxNetworkBehaviours} NetworkBehaviour components are allowed in order to save bandwidth.", this);
			}
		}
	}
}
