using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

namespace justbake.sfi
{
	public static class SmartFoxEventExtensions
	{
		#region params
		public static bool TryGetSuccessParam(this BaseEvent evt, out bool success) {
			if (evt.Params.TryGetValue("success", out object value) && value is bool successValue) {
				success = successValue;
				return true;
			}
			success = false;
			return false;
		}

		public static bool TryGetRoomParam(this BaseEvent evt, out Room room) {
			if (evt.Params.TryGetValue("room", out object value) && value is Room roomValue) {
				room = roomValue;
				return true;
			}
			room = null;
			return false;
		}

		public static bool TryGetUserParam(this BaseEvent evt, out User user) {
			if (evt.Params.TryGetValue("user", out object value) && value is User userValue) {
				user = userValue;
				return true;
			}
			user = null;
			return false;
		}

		public static bool TryGetErrorMessageParam(this BaseEvent evt, out string errorMessage) {
			if (evt.Params.TryGetValue("errorMessage", out object value) && value is string errorMessageValue) {
				errorMessage = errorMessageValue;
				return true;
			}
			errorMessage = "";
			return false;
		}
		public static bool TryGetReasonMessageParam(this BaseEvent evt, out string reason) {
			if (evt.Params.TryGetValue("reason", out object value) && value is string reasonValue) {
				reason = reasonValue;
				return true;
			}
			reason = "";
			return false;
		}

		public static bool TryGetErrorCodeParam(this BaseEvent evt, out short errorCode) {
			if (evt.Params.TryGetValue("errorCode", out object value) && value is short errorCodeValue) {
				errorCode = errorCodeValue;
				return true;
			}
			errorCode = 0;
			return false;
		}

		public static bool TryGetDataParam(this BaseEvent evt, out ISFSObject data) {
			if (evt.Params.TryGetValue("data", out object value) && value is ISFSObject dataValue) {
				data = dataValue;
				return true;
			}
			data = null;
			return false;
		}

		public static bool TryGetLagValueParam(this BaseEvent evt, out int lagValue) {
			if (evt.Params.TryGetValue("lagValue", out object value) && value is int lagValueValue) {
				lagValue = lagValueValue;
				return true;
			}
			lagValue = -1;
			return false;
		}
		#endregion

		#region Helper

		public static bool TryGetRoomUserIsIn(this SmartFox sfs, object id, out Room room) {
			if (sfs.RoomManager.ContainsRoom(id) || id != null) {
				Room tryGetRoom = null;
				if (id is int intid)
					tryGetRoom = sfs.RoomManager.GetRoomById(intid);
				if (id is string name)
					tryGetRoom = sfs.RoomManager.GetRoomByName(name);

				if (tryGetRoom != null && sfs.MySelf.IsJoinedInRoom(tryGetRoom)) {
					room = tryGetRoom;
					return true;
				}
			}
			room = null;
			return false;
		}

		#endregion
	}
}
