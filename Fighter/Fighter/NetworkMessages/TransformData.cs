using Fighter.Core;
using MemoryPack;

namespace Fighter.NetworkMessages
{
    [NetworkMessage("Transform")]
	[MemoryPackable]
	public partial class TransformData : NetworkMessage<TransformData>
	{
		public float pos_x;
		public float pos_y;
		public float pos_z;
		public float rot_x;
		public float rot_y;
		public float rot_z;
		public float scale_x;
		public float scale_y;
		public float scale_z;

		public static void OnReceive(int netid, string uid, TransformData msg)
		{
			TransformData td = msg;
			Console.WriteLine($"pos:({td.pos_x},{td.pos_y},{td.pos_z})");
		}
	}
}
