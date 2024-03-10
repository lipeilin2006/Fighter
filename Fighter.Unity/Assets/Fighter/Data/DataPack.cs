using MemoryPack;

namespace Data
{
	[MemoryPackable]
	public partial struct DataPack
	{
		public int rootID;
		public string UID;
		public string entityType;
		public string token;
		public string type;
		public byte[] data;
	}
}
