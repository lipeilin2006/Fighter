﻿using MemoryPack;

//这个命名空间中的东西要和Unity那边的一样，但也不是不能夹带私货，前提是不能影响序列化的数据结构。
namespace Data
{
	[MemoryPackable]
	public partial struct Pack
	{
		public string UID;
		public string token;
		public string route;
		public byte[] data;
	}
}