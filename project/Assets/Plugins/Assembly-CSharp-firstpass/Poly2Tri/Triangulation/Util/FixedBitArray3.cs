using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Poly2Tri.Triangulation.Util
{
	public struct FixedBitArray3 : IEnumerable, IEnumerable<bool>
	{
		[CompilerGenerated]
		private sealed class _003CEnumerate_003Ec__Iterator44 : IEnumerator, IDisposable, IEnumerable, IEnumerable<bool>, IEnumerator<bool>
		{
			internal int _003Ci_003E__0;

			internal int _0024PC;

			internal bool _0024current;

			internal FixedBitArray3 _003C_003Ef__this;

			bool IEnumerator<bool>.Current
			{
				[DebuggerHidden]
				get
				{
					return System_002ECollections_002EGeneric_002EIEnumerator_003Cbool_003E_002Eget_Current();
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return _0024current;
				}
			}

			[DebuggerHidden]
			private bool System_002ECollections_002EGeneric_002EIEnumerator_003Cbool_003E_002Eget_Current()
			{
				return _0024current;
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<bool>)this).GetEnumerator();
			}

			[DebuggerHidden]
			IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
			{
				if (Interlocked.CompareExchange(ref _0024PC, 0, -2) == -2)
				{
					return this;
				}
				return new _003CEnumerate_003Ec__Iterator44
				{
					_003C_003Ef__this = _003C_003Ef__this
				};
			}

			public bool MoveNext()
			{
				uint num = (uint)_0024PC;
				_0024PC = -1;
				switch (num)
				{
				case 0u:
					_003Ci_003E__0 = 0;
					goto IL_005e;
				case 1u:
					{
						_003Ci_003E__0++;
						goto IL_005e;
					}
					IL_005e:
					if (_003Ci_003E__0 < 3)
					{
						_0024current = _003C_003Ef__this[_003Ci_003E__0];
						_0024PC = 1;
						return true;
					}
					_0024PC = -1;
					break;
				}
				return false;
			}

			[DebuggerHidden]
			public void Dispose()
			{
				_0024PC = -1;
			}

			[DebuggerHidden]
			public void Reset()
			{
				throw new NotSupportedException();
			}
		}

		public bool _0;

		public bool _1;

		public bool _2;

		public bool this[int index]
		{
			get
			{
				switch (index)
				{
				case 0:
					return _0;
				case 1:
					return _1;
				case 2:
					return _2;
				default:
					throw new IndexOutOfRangeException();
				}
			}
			set
			{
				switch (index)
				{
				case 0:
					_0 = value;
					break;
				case 1:
					_1 = value;
					break;
				case 2:
					_2 = value;
					break;
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<bool> GetEnumerator()
		{
			return Enumerate().GetEnumerator();
		}

		public bool Contains(bool value)
		{
			for (int i = 0; i < 3; i++)
			{
				if (this[i] == value)
				{
					return true;
				}
			}
			return false;
		}

		public int IndexOf(bool value)
		{
			for (int i = 0; i < 3; i++)
			{
				if (this[i] == value)
				{
					return i;
				}
			}
			return -1;
		}

		public void Clear()
		{
			_0 = (_1 = (_2 = false));
		}

		public void Clear(bool value)
		{
			for (int i = 0; i < 3; i++)
			{
				if (this[i] == value)
				{
					this[i] = false;
				}
			}
		}

		[DebuggerHidden]
		private IEnumerable<bool> Enumerate()
		{
			//yield-return decompiler failed: Could not find currentField
			_003CEnumerate_003Ec__Iterator44 obj = new _003CEnumerate_003Ec__Iterator44();
			obj._003C_003Ef__this = this;
			obj._0024PC = -2;
			return obj;
		}
	}
}
