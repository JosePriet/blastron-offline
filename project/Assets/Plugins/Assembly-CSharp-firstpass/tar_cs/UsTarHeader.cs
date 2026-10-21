using System;
using System.Net;
using System.Text;

namespace tar_cs
{
	internal class UsTarHeader : TarHeader
	{
		private const string magic = "ustar";

		private const string version = "  ";

		private string groupName;

		private string namePrefix = string.Empty;

		private string userName;

		public override string UserName
		{
			get
			{
				return userName.Replace("\0", string.Empty);
			}
			set
			{
				if (value.Length > 32)
				{
					throw new TarException("user name can not be longer than 32 chars");
				}
				userName = value;
			}
		}

		public override string GroupName
		{
			get
			{
				return groupName.Replace("\0", string.Empty);
			}
			set
			{
				if (value.Length > 32)
				{
					throw new TarException("group name can not be longer than 32 chars");
				}
				groupName = value;
			}
		}

		public override string FileName
		{
			get
			{
				return namePrefix.Replace("\0", string.Empty) + base.FileName.Replace("\0", string.Empty);
			}
			set
			{
				if (value.Length > 100)
				{
					if (value.Length > 255)
					{
						throw new TarException("UsTar fileName can not be longer thatn 255 chars");
					}
					int num = value.Length - 100;
					while (!IsPathSeparator(value[num]))
					{
						num++;
						if (num == value.Length)
						{
							break;
						}
					}
					if (num == value.Length)
					{
						num = value.Length - 100;
					}
					namePrefix = value.Substring(0, num);
					base.FileName = value.Substring(num, value.Length - num);
				}
				else
				{
					base.FileName = value;
				}
			}
		}

		public override bool UpdateHeaderFromBytes()
		{
			byte[] bytes = GetBytes();
			UserName = Encoding.ASCII.GetString(bytes, 265, 32);
			GroupName = Encoding.ASCII.GetString(bytes, 297, 32);
			namePrefix = Encoding.ASCII.GetString(bytes, 347, 157);
			return base.UpdateHeaderFromBytes();
		}

		private static bool IsPathSeparator(char ch)
		{
			return ch == '\\' || ch == '/' || ch == '|';
		}

		public override byte[] GetHeaderValue()
		{
			byte[] headerValue = base.GetHeaderValue();
			Encoding.ASCII.GetBytes("ustar").CopyTo(headerValue, 257);
			Encoding.ASCII.GetBytes("  ").CopyTo(headerValue, 262);
			Encoding.ASCII.GetBytes(UserName).CopyTo(headerValue, 265);
			Encoding.ASCII.GetBytes(GroupName).CopyTo(headerValue, 297);
			Encoding.ASCII.GetBytes(namePrefix).CopyTo(headerValue, 347);
			if (SizeInBytes >= 8589934591L)
			{
				byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(SizeInBytes));
				SetMarker(AlignTo12(bytes)).CopyTo(headerValue, 124);
			}
			RecalculateChecksum(headerValue);
			Encoding.ASCII.GetBytes(base.HeaderChecksumString).CopyTo(headerValue, 148);
			return headerValue;
		}

		private static byte[] SetMarker(byte[] bytes)
		{
			bytes[0] |= 128;
			return bytes;
		}

		private static byte[] AlignTo12(byte[] bytes)
		{
			byte[] array = new byte[12];
			bytes.CopyTo(array, 12 - bytes.Length);
			return array;
		}
	}
}
