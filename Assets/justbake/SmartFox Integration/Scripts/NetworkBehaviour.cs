using System.Collections;
using UnityEngine;

namespace justbake.sfi {
    public abstract class NetworkBehaviour : MonoBehaviour {

        /// <summary>Returns the NetworkIdentity of this object</summary>
        public NetworkIdentity netIdentity {
            get; internal set;
        }

        /// <summary>Returns the index of the component on this object</summary>
        public byte ComponentIndex {
            get; internal set;
        }

        /// <summary>The unique network Id of this object (unique at runtime).</summary>
        public int netId => netIdentity.Id;

        /// <summary>True if this object is the the client's own local player.</summary>
        public bool IsLocalPlayer => netIdentity.IsLocalUser;
    }
}