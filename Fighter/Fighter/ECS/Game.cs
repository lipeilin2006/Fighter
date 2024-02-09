using Data.Funcs;
using Fighter.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Fighter.ECS
{
    public static class Game
	{
		public static FighterServer? Server { get; private set; }
		public static ConcurrentDictionary<int, Root> Roots { get; set; } = new();

		static System.Timers.Timer timer = new();

		static Random random = new();

		public static void Init()
		{
			Server = new();

			foreach (MethodInfo info in typeof(DataDeserializationFuncs).GetMethods())
			{
				Server.DeserializeFuncs.TryAdd(info.Name, (byte[] data) => { return info.Invoke(null, [data]); });
			}
			foreach (MethodInfo info in typeof(DataActionFuncs).GetMethods())
			{
				Server.DataActions.TryAdd(info.Name, (netid, uid, entityType, obj) => { info.Invoke(null, [netid, uid, entityType, obj]); });
			}
			Server.Start();
			timer.Interval = 30;
			timer.Enabled = true;
			timer.Elapsed += OnNetworkUpdate;
			timer.Start();
		}
		private static void OnNetworkUpdate(object? sender, ElapsedEventArgs e)
		{
			foreach (Root root in Roots.Values)
			{
				root.OnNetworkUpdate();
			}
		}
		private static int GenerateRootID()
		{
			int id;
			while (true)
			{
				id = random.Next();
				if (!Roots.ContainsKey(id)) break;
			}
			return id;
		}
		public static int CreateRoot(string key = "")
		{
			int id = GenerateRootID();
			Roots.TryAdd(id, new Root(Server, key));
			return id;
		}
		public static void CreateRoot(int id, string key = "")
		{
			Roots.TryAdd(id, new Root(Server, key));
		}
		public static void RemoveRoot(int id)
		{
			Roots.TryRemove(id, out _);
		}
	}
}
