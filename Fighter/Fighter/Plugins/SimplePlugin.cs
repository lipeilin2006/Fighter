using Fighter.Core;
using kcp2k;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fighter.Plugins
{
	public class SimplePlugin : FighterPlugin
	{
		public override void OnConnected(int id)
		{
			Console.WriteLine($"{id} Connected");
		}

		public override void OnData(int id, ArraySegment<byte> data, KcpChannel channel)
		{
			Console.WriteLine($"Recv ID:{id},Length:{data.Count},Channel:{channel}");
		}

		public override void OnDisconnected(int id)
		{
			Console.WriteLine($"{id} Disconnected");
		}

		public override void OnError(int id, ErrorCode error, string msg)
		{
			Console.WriteLine($"{id} Error");
		}
	}
}
