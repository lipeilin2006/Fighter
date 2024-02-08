using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fighter.Server
{
	public class Player(string uid,string token, int net_id)
	{
		public string UID { get; } = uid;
		public string Token { get; } = token;
		public int NetID { get; } = net_id;
		//留着用来加自定义数据的
		public ConcurrentDictionary<string, object> CustomConfigs { get; set; } = new();
	}
}
