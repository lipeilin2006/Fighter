using MemoryPack.Compression;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;

namespace Data.Funcs
{
    public static class DataDeserializationFuncs
    {
        public static object Transform(byte[] data)
        {
            BrotliDecompressor decompressor = new();
            data = decompressor.Decompress(data).ToArray();
            return MemoryPackSerializer.Deserialize<TransformData>(data.ToArray());
        }
        public static object JoinRootRequest(byte[] data)
        {
			BrotliDecompressor decompressor = new();
			data = decompressor.Decompress(data).ToArray();
			return MemoryPackSerializer.Deserialize<JoinRootRequestData>(data.ToArray());
		}
        public static object JoinRootCallback(byte[] data)
        {
			BrotliDecompressor decompressor = new();
			data = decompressor.Decompress(data).ToArray();
            return MemoryPackSerializer.Deserialize<JoinRootCallbackData>(data.ToArray());
		}
	}
}
