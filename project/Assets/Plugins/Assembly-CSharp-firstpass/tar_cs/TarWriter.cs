using System;
using System.IO;

namespace tar_cs
{
	public class TarWriter : LegacyTarWriter
	{
		public TarWriter(Stream writeStream)
			: base(writeStream)
		{
		}

		protected override void WriteHeader(string name, DateTime lastModificationTime, long count, int userId, int groupId, int mode)
		{
			UsTarHeader usTarHeader = new UsTarHeader();
			usTarHeader.FileName = name;
			usTarHeader.LastModification = lastModificationTime;
			usTarHeader.SizeInBytes = count;
			usTarHeader.UserId = userId;
			usTarHeader.UserName = Convert.ToString(userId, 8);
			usTarHeader.GroupId = groupId;
			usTarHeader.GroupName = Convert.ToString(groupId, 8);
			usTarHeader.Mode = mode;
			UsTarHeader usTarHeader2 = usTarHeader;
			OutStream.Write(usTarHeader2.GetHeaderValue(), 0, usTarHeader2.HeaderSize);
		}

		protected virtual void WriteHeader(string name, DateTime lastModificationTime, long count, string userName, string groupName, int mode)
		{
			UsTarHeader usTarHeader = new UsTarHeader();
			usTarHeader.FileName = name;
			usTarHeader.LastModification = lastModificationTime;
			usTarHeader.SizeInBytes = count;
			usTarHeader.UserId = userName.GetHashCode();
			usTarHeader.UserName = userName;
			usTarHeader.GroupId = groupName.GetHashCode();
			usTarHeader.GroupName = groupName;
			usTarHeader.Mode = mode;
			UsTarHeader usTarHeader2 = usTarHeader;
			OutStream.Write(usTarHeader2.GetHeaderValue(), 0, usTarHeader2.HeaderSize);
		}

		public virtual void Write(string name, long dataSizeInBytes, string userName, string groupName, int mode, DateTime lastModificationTime, WriteDataDelegate writeDelegate)
		{
			DataWriter dataWriter = new DataWriter(OutStream, dataSizeInBytes);
			WriteHeader(name, lastModificationTime, dataSizeInBytes, userName, groupName, mode);
			while (dataWriter.CanWrite)
			{
				writeDelegate(dataWriter);
			}
			AlignTo512(dataSizeInBytes, false);
		}

		public void Write(Stream data, long dataSizeInBytes, string fileName, string userId, string groupId, int mode, DateTime lastModificationTime)
		{
			WriteHeader(fileName, lastModificationTime, dataSizeInBytes, userId, groupId, mode);
			WriteContent(dataSizeInBytes, data);
			AlignTo512(dataSizeInBytes, false);
		}
	}
}
