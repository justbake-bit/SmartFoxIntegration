using UnityEditor;
using UnityEngine;

namespace justbake.sfi
{
	[CustomEditor(typeof(NetworkManager), true)]
	[CanEditMultipleObjects]
	public class NetowrkManagerEditor : Editor
	{
		protected NetworkManager networkManager;

		protected void Init() {
			networkManager = target as NetworkManager;
		}

		public override void OnInspectorGUI() {
			Init();
			DrawDefaultInspector();

			if (Application.isPlaying) {
				if (!networkManager.IsConnected && GUILayout.Button("Connect")) {
					networkManager.Connect();
				}

				if (networkManager.IsConnected && GUILayout.Button("Disconnect")) {
					networkManager.Disconnect();
				}
			}
		}
	}
}
