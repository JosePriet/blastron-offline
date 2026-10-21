using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace EB.Sparx
{
	public class GameCenterManager : SubSystem
	{
		private GameCenterAPI _api;

		private GameCenterConfig _config;

		private bool _isAuthenticating;

		public override void Initialize(Config config)
		{
			_config = config.GameCenterConfig;
			_api = new GameCenterAPI(base.Hub.ApiEndPoint);
		}

		public override IEnumerator LoadOfflineData()
		{
			ConnectToGameCenter();
			yield break;
		}

		public override void Connect()
		{
			base.State = SubSystemState.Connected;
			ConnectToGameCenter();
		}

		public void ReportScore(int leaderboardId, int score)
		{
			if (!string.IsNullOrEmpty(_config.LeaderboardPrefix))
			{
				Debug.Log("Report score: " + leaderboardId + " " + score);
			}
		}

		private void OnReportScore(bool success)
		{
			Debug.Log("OnReportScore: " + success);
		}

		public void ReportProgress(string achievementId, double progress)
		{
			if (!string.IsNullOrEmpty(_config.AchievementPrefix))
			{
				if (progress > 100.0)
				{
					progress = 100.0;
				}
				if (progress < 0.0)
				{
					progress = 0.0;
				}
				Debug.Log("Report progress: " + achievementId + " " + progress);
			}
		}

		private void OnReportProgress(bool success)
		{
			Debug.Log("OnReportProgress: " + success);
		}

		public override void Disconnect(bool isLogout)
		{
			base.State = SubSystemState.Disconnected;
		}

		public void ShowLeaderboardUI()
		{
			Debug.Log("ShowLeaderboardUI");
			Social.ShowLeaderboardUI();
		}

		public void ShowAchievementsUI()
		{
			Debug.Log("ShowAchievementsUI");
			Social.ShowAchievementsUI();
		}

		private void ConnectToGameCenter()
		{
			ILocalUser localUser = Social.localUser;
			if (localUser != null)
			{
				if (localUser.authenticated)
				{
					OnAuthenticate(true);
				}
				else if (!_isAuthenticating)
				{
					_isAuthenticating = true;
					localUser.Authenticate(OnAuthenticate);
				}
			}
		}

		private void OnSetGameCenterId(string error)
		{
		}

		private void OnAuthenticate(bool success)
		{
			_isAuthenticating = false;
			Debug.Log("On Gamecenter auth " + success);
			if (!success || base.State != SubSystemState.Connected)
			{
				return;
			}
			User localUser = base.Hub.LoginManager.LocalUser;
			if (localUser != null)
			{
				string gameCenterId = localUser.GameCenterId;
				if (gameCenterId != Social.localUser.id)
				{
					_api.SetGameCenterId(Social.localUser.id, Social.localUser.userName, OnSetGameCenterId);
				}
			}
		}
	}
}
