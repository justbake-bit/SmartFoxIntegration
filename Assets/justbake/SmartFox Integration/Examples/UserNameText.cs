using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace justbake.sfi
{
	[RequireComponent(typeof(TMP_Text))]
	public class UserNameText : MonoBehaviour
	{
		[SerializeField] NetworkUser user;
		TMP_Text text;

		private void Awake() {
			text = GetComponent<TMP_Text>();
		}

		private void Start() {
			if (user.IsLocalUser) {
				text.color = Color.green;
			} else {
				text.color = Color.red;
			}
			text.text = user.Name;
		}
	}
}
