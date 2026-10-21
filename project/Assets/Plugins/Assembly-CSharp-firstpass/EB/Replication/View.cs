using System;
using System.Reflection;
using EB.Sparx;
using UnityEngine;

namespace EB.Replication
{
	[AddComponentMenu("Replication/View")]
	public class View : MonoBehaviour
	{
		private enum TransformFlags
		{
			P_X = 1,
			P_Y = 2,
			P_Z = 4,
			R_X = 8,
			R_Y = 0x10,
			R_Z = 0x20
		}

		public Component observed;

		public NetworkStateSynchronization stateSynchronization;

		public NetworkMigrationSynchronization migrationSynchronization;

		public float updateInterval;

		private float _lastUpdate;

		public ViewId viewId { get; private set; }

		public uint instantiatorId { get; private set; }

		public int lastHash { get; set; }

		public bool isMine
		{
			get
			{
				return Manager.IsMine(viewId);
			}
		}

		public uint ownerId
		{
			get
			{
				return viewId.p;
			}
		}

		public Player ownerPlayer
		{
			get
			{
				return Manager.GetPlayer(viewId.p);
			}
		}

		public Player instantiatorPlayer
		{
			get
			{
				return Manager.GetPlayer(instantiatorId);
			}
		}

		private void Awake()
		{
			viewId = Manager.Register(this);
			instantiatorId = viewId.p;
		}

		private void OnDestroy()
		{
			Manager.Unregister(this);
		}

		public void TransferOwnership()
		{
			Manager.Unregister(this);
			viewId = Manager.Register(this);
			if (observed != null)
			{
				observed.SendMessage("OnTransferedOwnership", SendMessageOptions.DontRequireReceiver);
			}
		}

		public void RPC(string name, RPCMode mode, params object[] args)
		{
			if (observed != null)
			{
				Type type = observed.GetType();
				if (type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
				{
					Manager.ViewRPC(name, viewId, mode, args);
					return;
				}
				Debug.LogWarning("skipping rpc (" + name + ") on type  " + observed.GetType());
			}
		}

		public void Serialize(BitStream bs)
		{
			if (observed == null)
			{
				throw new Exception("View Is in a invalid state " + viewId);
			}
			if (bs.isWriting)
			{
				if (updateInterval > 0f && Time.realtimeSinceStartup - _lastUpdate < updateInterval)
				{
					return;
				}
				_lastUpdate = Time.realtimeSinceStartup;
			}
			if (observed is Transform)
			{
				Transform transform = (Transform)observed;
				if (bs.isWriting)
				{
					Vector3 localPosition = transform.localPosition;
					Vector3 eulerAngles = transform.localRotation.eulerAngles;
					int index = bs.Reserve();
					int num = 0;
					if (localPosition.x != 0f)
					{
						num |= 1;
						bs.Serialize(ref localPosition.x);
					}
					if (localPosition.y != 0f)
					{
						num |= 2;
						bs.Serialize(ref localPosition.y);
					}
					if (localPosition.z != 0f)
					{
						num |= 4;
						bs.Serialize(ref localPosition.z);
					}
					if (eulerAngles.x != 0f)
					{
						num |= 8;
						bs.Serialize(ref eulerAngles.x);
					}
					if (eulerAngles.y != 0f)
					{
						num |= 0x10;
						bs.Serialize(ref eulerAngles.y);
					}
					if (eulerAngles.z != 0f)
					{
						num |= 0x20;
						bs.Serialize(ref eulerAngles.z);
					}
					bs.Poke(index, (byte)num);
				}
				else
				{
					byte data = 0;
					bs.Serialize(ref data);
					Vector3 zero = Vector3.zero;
					Vector3 zero2 = Vector3.zero;
					if ((data & 1) != 0)
					{
						bs.Serialize(ref zero.x);
					}
					if ((data & 2) != 0)
					{
						bs.Serialize(ref zero.y);
					}
					if ((data & 4) != 0)
					{
						bs.Serialize(ref zero.z);
					}
					if ((data & 8) != 0)
					{
						bs.Serialize(ref zero2.x);
					}
					if ((data & 0x10) != 0)
					{
						bs.Serialize(ref zero2.y);
					}
					if ((data & 0x20) != 0)
					{
						bs.Serialize(ref zero2.z);
					}
					transform.localPosition = zero;
					transform.localRotation = Quaternion.Euler(zero2);
				}
			}
			else
			{
				if (!(observed is MonoBehaviour))
				{
					throw new Exception("Unsupported observed type: " + observed.GetType());
				}
				MonoBehaviour monoBehaviour = (MonoBehaviour)observed;
				monoBehaviour.GetType().InvokeMember("OnSerializeView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, monoBehaviour, new object[1] { bs });
			}
		}
	}
}
