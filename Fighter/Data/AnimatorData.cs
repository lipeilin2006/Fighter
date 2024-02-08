using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
	[MemoryPackable]
	public partial struct AnimatorData
	{
		public LayerData[] layerDatas;
		public float[] layerWeights;
	}

	[MemoryPackable]
	public partial struct LayerData
	{
		public int fullPathHash;
		public float normalizedTime;
	}
}
