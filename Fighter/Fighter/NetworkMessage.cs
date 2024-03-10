using MemoryPack;

namespace Data
{
	public abstract class NetworkMessage<T>
	{
		public byte[] Serialize()
		{
			return MemoryPackSerializer.Serialize(this);
		}
		public static T? Deserialize(byte[] data)
		{
			return MemoryPackSerializer.Deserialize<T>(data);
		}
	}
	public class NetworkMessageAttribute(string type) : Attribute
	{
		public string MessageType = type;
	}
}
