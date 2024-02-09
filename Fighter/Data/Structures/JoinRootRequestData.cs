using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
	[MemoryPackable]
	public partial struct JoinRootRequestData
	{
		public int rootID;
		public string key;
	}
}
