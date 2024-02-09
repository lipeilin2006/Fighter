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
			//反射获取DataDeserializationFuncs里面的所有函数并注册
			foreach (MethodInfo info in typeof(DataDeserializationFuncs).GetMethods())
			{
				Server.DeserializeFuncs.TryAdd(info.Name, (byte[] data) => { return info.Invoke(null, [data]); });
			}
			//反射获取DataActionFuncs里面的所有函数并注册
			foreach (MethodInfo info in typeof(DataActionFuncs).GetMethods())
			{
				Server.DataActions.TryAdd(info.Name, (netid, uid, entityType, obj) => { info.Invoke(null, [netid, uid, entityType, obj]); });
			}
			//启动服务器
			Server.Start();
			//启动计时器，用于定时调用OnNetworkUpdate
			timer.Interval = 30;
			timer.Enabled = true;
			timer.Elapsed += OnNetworkUpdate;
			timer.Start();
		}
		/// <summary>
		/// 调用Root里面的OnNetworkUpdate
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void OnNetworkUpdate(object? sender, ElapsedEventArgs e)
		{
			foreach (Root root in Roots.Values)
			{
				root.OnNetworkUpdate();
			}
		}
		/// <summary>
		/// RootID必须全局唯一，因此用这个函数统一生成
		/// </summary>
		/// <returns></returns>
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
		/// <summary>
		/// 创建Root
		/// </summary>
		/// <param name="key">加入这个Root节点所需的密钥</param>
		/// <returns></returns>
		public static int CreateRoot(string key = "")
		{
			int id = GenerateRootID();
			Roots.TryAdd(id, new Root(Server, key));
			return id;
		}
		/// <summary>
		/// 创建Root
		/// </summary>
		/// <param name="id">自定义RootID，一般是大厅等保留ID，要保证RootID全局唯一</param>
		/// <param name="key"></param>
		public static void CreateRoot(int id, string key = "")
		{
			Roots.TryAdd(id, new Root(Server, key));
		}
		/// <summary>
		/// 移除Root
		/// </summary>
		/// <param name="id">RootID</param>
		public static void RemoveRoot(int id)
		{
			Root? root;
			Roots.TryRemove(id, out root);
			root?.Dispose();
		}
	}
}
