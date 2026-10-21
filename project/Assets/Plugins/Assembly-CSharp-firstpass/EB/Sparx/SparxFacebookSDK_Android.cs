using System.Collections;
using UnityEngine;

namespace EB.Sparx
{
	public class SparxFacebookSDK_Android : MonoBehaviour
	{
		public EventEmitter Emitter = new EventEmitter();

		private void OnAuthorize(string access_token)
		{
			Debug.Log("!*!*!*Facebook Authorize: Success - access_token = " + access_token);
			Hashtable hashtable = new Hashtable();
			hashtable.Add("access_token", access_token);
			object arg = hashtable;
			Emitter.Emit("authorize", arg);
		}

		private void OnAuthorizeFailed(string cancel)
		{
			bool result = false;
			bool.TryParse(cancel, out result);
			string empty = string.Empty;
			if (result)
			{
				empty = "ID_SPARX_ERROR_FACEBOOK_CANCELED";
				Debug.Log("!*!*!*Facebook Authorize: Failed - Cancelled by user");
			}
			else
			{
				empty = "ID_SPARX_ERROR_FACEBOOK";
				Debug.LogError("!*!*!*Facebook Authorize: Failed - Facebook error while authorizing");
			}
			Hashtable hashtable = new Hashtable();
			hashtable.Add("err", empty);
			object arg = hashtable;
			Emitter.Emit("authorize", arg);
		}

		private void OnStreamPostSuccess(string values)
		{
			Debug.Log("!*!*!*Facebook StreamPost: Success - values = " + values);
		}

		private void OnStreamPostFailed(string error)
		{
			Debug.LogError("!*!*!*Facebook StreamPost: Failed - error = " + error);
		}

		private void OnRequestSuccess(string values)
		{
			Debug.Log("!*!*!*Facebook Request: Success - values = " + values);
		}

		private void OnRequestFailed(string error)
		{
			Debug.LogError("!*!*!*Facebook Request: Failed - error = " + error);
		}

		private void OnInviteSuccess(string values)
		{
			Debug.Log("!*!*!*Facebook Invite: Success - values = " + values);
		}

		private void OnInviteFailed(string error)
		{
			Debug.LogError("!*!*!*Facebook Invite: Failed - error = " + error);
		}

		private void OnDialog(string data)
		{
			Debug.Log("!*!*!*Facebook OnDialog - data = " + data);
		}
	}
}
