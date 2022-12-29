using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace justbake.sfi
{
	public class UsernameText : NetworkUserBehaviour
	{
		[SerializeField] private TMP_Text nameText;
		public void Start() {
			nameText.text = netUserIdentity.Name;
		}
	}
}
