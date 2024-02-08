using MemoryPack;

namespace Data
{
	[MemoryPackable]
	public partial struct DataPack
	{
		public string UID;
		public string Token;
		public string type;
		public byte[] data;
	}
}
