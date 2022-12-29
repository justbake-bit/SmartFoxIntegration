using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace justbake.sfi
{
	[RequireComponent(typeof(NetworkUser))]
	public class NetworkUserBehaviour : MonoBehaviour
	{
		/// <summary>
		/// Returns the NetworkIdentity of this object
		/// </summary>
		public NetworkUser netUserIdentity {
			get; internal set;
		}

		/// <summary>Returns the index of the component on this object</summary>
		public byte ComponentIndex {
			get; internal set;
		}

		/// <summary>True if this object is the the client's own local player.</summary>
		public bool isLocalUser => netUserIdentity.IsLocalUser;

		/// <summary>isOwned is true on the client if this NetworkIdentity is one of the .owned entities of our connection on the server.</summary>
		// for example: main player & pets are owned. monsters & npcs aren't.
		public bool IsOwned => netUserIdentity.IsOwned;

		public int Id => netUserIdentity.Id;

		/// <summary>Like Start(), but only called on client and host.</summary>
		public virtual void OnStartClient() {
		}

		/// <summary>Stop event, only called on client and host.</summary>
		public virtual void OnStopClient() {
		}

		/// <summary>Like Start(), but only called on client and host for the local player object.</summary>
		public virtual void OnStartLocalPlayer() {
		}

		/// <summary>Stop event, but only called on client and host for the local player object.</summary>
		public virtual void OnStopLocalPlayer() {
		}

		/// <summary>Like Start(), but only called for objects the client has authority over.</summary>
		public virtual void OnStartAuthority() {
		}

		/// <summary>Stop event, only called for objects the client has authority over.</summary>
		public virtual void OnStopAuthority() {
		}
	}
}
