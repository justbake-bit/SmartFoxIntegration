using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Diagnostics;

namespace justbake.sfi
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Network/Network Identity")]
	public class NetworkIdentity : MonoBehaviour
	{
		public int Id {
			get; internal set;
		}

		public string Name {
			get; internal set;
		}

		public int SceneId {
			get; internal set;
		}
		public bool IsLocalUser => NetworkManager.Instance.LocalUser.Id == Id;

	}
}
