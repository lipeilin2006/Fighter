using MemoryPack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
