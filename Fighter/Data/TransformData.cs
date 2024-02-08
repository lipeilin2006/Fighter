using MemoryPack;
using MemoryPack.Compression;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
	[MemoryPackable]
	public partial struct TransformData
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

		public static object Deserlize(byte[] data)
		{
			BrotliDecompressor decompressor = new();
			data = decompressor.Decompress(data).ToArray();
			return MemoryPackSerializer.Deserialize<TransformData>(data.ToArray());
		}
	}
}
