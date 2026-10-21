using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EB.Sparx
{
	public class Hub : MonoBehaviour
	{
		private List<Manager> _managers = new List<Manager>();

		private List<SubSystem> _subsystems = new List<SubSystem>();

		private List<Updatable> _update = new List<Updatable>();

		private bool _wasLoggedIn;

		private int _enteredBackground;

		public static Hub Instance { get; private set; }

		public Config Config { get; private set; }

		public HubState State { get; private set; }

		public EndPoint ApiEndPoint { get; private set; }

		public LoginManager LoginManager { get; private set; }

		public UserManager UserManager { get; private set; }

		public GameManager GameManager { get; private set; }

		public DataManager DataManager { get; private set; }

		public SaveLoadManager SaveLoadManager { get; private set; }

		public PushManager PushManager { get; private set; }

		public InventoryManager InventoryManager { get; private set; }

		public WalletManager WalletManager { get; private set; }

		public GameCenterManager GameCenterManager { get; private set; }

		public TelemetryManager TelemetryManager { get; private set; }

		public LeaderboardManager LeaderboardManager { get; private set; }

		public TuningManager TuningManager { get; private set; }

		public FacebookManager FacebookManager { get; private set; }

		public AppiraterManager AppiraterManager { get; private set; }

		public GachaManager GachaManager { get; private set; }

		public event Action OnUpdate;

		public static Hub Create(Config config)
		{
			if (Instance != null)
			{
				throw new ApplicationException("Sparx is already initialized");
			}
			new GameObject("sparxhub", config.HubType);
			Instance.Initialize(config);
			return Instance;
		}

		private void Initialize(Config config)
		{
			Config = config;
			BugReport.Init(config.ApiEndpoint + "/bugs");
			if (config.LoadLocalizer)
			{
				Localizer.LoadStrings(Config.Locale, "sparx");
			}
			TextAsset textAsset = Assets.Load<TextAsset>("profanity");
			if (textAsset != null)
			{
				ProfanityFilter.Init(textAsset.text);
			}
			EndPointOptions options = new EndPointOptions
			{
				Key = config.ApiKey.Value
			};
			if (config.ApiKeepAlive > 0)
			{
				options.KeepAlive = true;
				options.KeepAliveUrl = "/util/ping";
				options.KeepAliveInterval = config.ApiKeepAlive;
			}
			ApiEndPoint = EndPointFactory.Create(config.ApiEndpoint, options);
			Memory.OnBreach += () =>
			{
				FatalError("ID_SPARX_ERROR_UNKNOWN");
			};
			InitializeComponents();
		}

		public Coroutine LoadOffineData()
		{
			return StartCoroutine(_LoadOffineData());
		}

		private IEnumerator _LoadOffineData()
		{
			foreach (Manager manager in _managers)
			{
				yield return StartCoroutine(manager.LoadOfflineData());
			}
			Config.LoginConfig.Listener.OnOfflineDataLoaded();
		}

		protected virtual void InitializeComponents()
		{
			TuningManager = AddManager<TuningManager>();
			TelemetryManager = AddManager<TelemetryManager>();
			UserManager = AddManager<UserManager>();
			LoginManager = AddManager<LoginManager>(Config.LoginConfig.LoginManagerType);
			DataManager = AddManager<DataManager>();
			SaveLoadManager = AddManager<SaveLoadManager>();
			WalletManager = AddManager<WalletManager>();
			LeaderboardManager = AddManager<LeaderboardManager>();
			FacebookManager = AddManager<FacebookManager>();
			AppiraterManager = AddManager<AppiraterManager>();
			GachaManager = AddManager<GachaManager>();
			if (Config.GameCenterConfig.Enabled)
			{
				GameCenterManager = AddManager<GameCenterManager>();
			}
			if (Config.GameManagerConfig.Enabled)
			{
				GameManager = AddManager<GameManager>();
			}
			if (Config.UsePush)
			{
				PushManager = AddManager<PushManager>();
			}
			if (Config.InventoryConfig.Enabled)
			{
				InventoryManager = AddManager<InventoryManager>();
			}
			foreach (Type gameComponent in Config.GameComponents)
			{
				AddManager<Manager>(gameComponent);
			}
		}

		protected T AddManager<T>() where T : Manager, new()
		{
			return AddManager<T>(typeof(T));
		}

		protected T AddManager<T>(Type type) where T : Manager
		{
			T val = (T)Activator.CreateInstance(type);
			val.Initialize(Config);
			AddManager(val);
			return val;
		}

		public Manager GetManager(string name)
		{
			string text = "EB.Sparx." + name;
			foreach (Manager manager in _managers)
			{
				string text2 = manager.GetType().Name;
				if (text2 == name || text2 == text)
				{
					return manager;
				}
			}
			return null;
		}

		public Manager GetManager(Type type)
		{
			foreach (Manager manager in _managers)
			{
				if (manager.GetType() == type || manager.GetType().IsSubclassOf(type))
				{
					return manager;
				}
			}
			return null;
		}

		public T GetManager<T>() where T : Manager
		{
			return (T)GetManager(typeof(T));
		}

		private void AddManager(Manager manager)
		{
			_managers.Add(manager);
			if (manager is SubSystem)
			{
				_subsystems.Add((SubSystem)manager);
			}
			if (manager is Updatable)
			{
				_update.Add((Updatable)manager);
			}
		}

		private void OnDestroy()
		{
			ApiEndPoint.Dispose();
			foreach (Manager manager in _managers)
			{
				manager.Dispose();
			}
		}

		private void OnLoginStateChanged(LoginState state)
		{
			if (state == LoginState.LoggedIn)
			{
				SubSystemConnect();
			}
		}

		public void FatalError(string error)
		{
			Debug.LogError("Sparx Fatal Error: " + error);
			bool flag = State == HubState.Connected;
			State = HubState.Idle;
			if (Config.LoginConfig.Listener != null)
			{
				if (flag)
				{
					Config.LoginConfig.Listener.OnDisconnected(error);
				}
				else
				{
					bool isSilentLoginFlow = LoginManager.IsSilentLoginFlow;
					Config.LoginConfig.Listener.OnLoginFailed(error, isSilentLoginFlow, true);
				}
				Config.LoginConfig.Listener.EnableInput(true);
			}
			SubSystemDisconnect(false);
		}

		public void Disconnect(bool isLogout)
		{
			State = HubState.Idle;
			if (Config.LoginConfig.Listener != null)
			{
				Config.LoginConfig.Listener.OnDisconnected(string.Empty);
			}
			SubSystemDisconnect(isLogout);
			if (isLogout)
			{
				LoadOffineData();
			}
		}

		private void SubSystemConnect()
		{
			Debug.Log("SparxHub: SubSystemConnect");
			Config.LoginConfig.Listener.EnableInput(false);
			State = HubState.Connecting;
			foreach (SubSystem subsystem in _subsystems)
			{
				subsystem.State = SubSystemState.Connecting;
				subsystem.Connect();
			}
		}

		private void SubSystemConnecting()
		{
			bool flag = true;
			foreach (SubSystem subsystem in _subsystems)
			{
				switch (subsystem.State)
				{
				case SubSystemState.Error:
					FatalError(Localizer.GetString("ID_SPARX_ERROR_UNKNOWN"));
					return;
				case SubSystemState.Disconnected:
				case SubSystemState.Connecting:
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				return;
			}
			Debug.Log("SparxHub: SubSystemConnected");
			State = HubState.Connected;
			ApiEndPoint.StartKeepAlive();
			if (Config.LoginConfig.Listener != null)
			{
				Config.LoginConfig.Listener.OnLoggedIn();
			}
			foreach (Manager manager in _managers)
			{
				manager.OnLoggedIn();
			}
			Config.LoginConfig.Listener.EnableInput(true);
		}

		private void SubSystemUpdate()
		{
			bool flag = State != HubState.Idle;
			foreach (Updatable item in _update)
			{
				if (item.UpdateOffline || flag)
				{
					item.Update();
				}
			}
		}

		private void SubSystemDisconnect(bool isLogout)
		{
			Debug.Log("SparxHub: SubSystemDisconnect");
			foreach (SubSystem subsystem in _subsystems)
			{
				subsystem.Disconnect(isLogout);
				subsystem.State = SubSystemState.Disconnected;
			}
			ApiEndPoint.StopKeepAlive();
		}

		private void Update()
		{
			SubSystemUpdate();
			HubState state = State;
			if (state == HubState.Connecting)
			{
				SubSystemConnecting();
			}
			if (OnUpdate != null)
			{
				OnUpdate();
			}
		}

		private void Awake()
		{
			Instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}

		private void OnApplicationPause(bool pause)
		{
			Debug.LogError("OnApplicationPause:  " + pause);
			if (!Config.BackgroundOnPause || !pause)
			{
				return;
			}
			Debug.LogError("SparxHub::OnEnteredBackground ");
			foreach (Manager manager in _managers)
			{
				manager.OnEnteredBackground();
			}
		}
	}
}
