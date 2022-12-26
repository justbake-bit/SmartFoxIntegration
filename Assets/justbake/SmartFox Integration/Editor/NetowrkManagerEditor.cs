using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace justbake.sfi
{
	[CustomEditor(typeof(NetworkManager), true)]
	[CanEditMultipleObjects]
	public class NetowrkManagerEditor : Editor
	{
		protected NetworkManager networkManager;
		protected string roomName = "";
		protected string roomPassword = "";
		protected bool spectate;
		protected int id = 0;

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

				if (networkManager.IsLoggedIn) {
					roomName = GUILayout.TextField(roomName);
					spectate = GUILayout.Toggle(spectate, "Spectate");
					if (GUILayout.Button("Join")) {
						networkManager.JoinRoom(id: roomName, password: roomPassword, asSpectator: spectate);
					}
					if (GUILayout.Button("Leave")) {
						networkManager.LeaveRoom(roomName);
					}
				}
			}
		}
	}
}
