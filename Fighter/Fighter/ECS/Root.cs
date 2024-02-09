using Fighter.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Fighter.ECS
{
    public class Root
	{
		public FighterServer? Server { get; }
		public ConcurrentDictionary<(string, string), Entity> Entities { get; set; } = new();
		public string Key { get; set; }

		public Root(FighterServer? server,string key)
		{
			Server = server;
			Key = key;
		}
		/// <summary>
		/// 调用Entity中的OnNetworkUpdate
		/// </summary>
		public void OnNetworkUpdate()
		{
			foreach (Entity entity in Entities.Values)
			{
				entity.OnNetworkUpdate();
			}
		}
		/// <summary>
		/// 添加Entity到该Root节点
		/// </summary>
		/// <param name="entityType"></param>
		/// <param name="uid">默认为空是用于某些由服务端控制的Entity</param>
		/// <returns></returns>
		public Entity? AddEntity(string entityType, string uid = "")
		{

			if (Entities.ContainsKey((uid, entityType))) return null;

			Entity entity = new(uid, entityType, this);
			Entities.TryAdd((uid, entityType), entity);
			return entity;
		}
		/// <summary>
		/// 通过玩家UID模糊查找Entity
		/// </summary>
		/// <param name="uid">UID</param>
		/// <returns>所有匹配的Entity</returns>
		public Entity[] FindEntitiesByUID(string uid)
		{
			return Entities.Values.Where((entity) => { if (entity.UID == uid) return true; return false; }).ToArray();
		}
		/// <summary>
		/// 通过Entity的类型模糊查找Entity
		/// </summary>
		/// <param name="type">Entity Type</param>
		/// <returns>所有匹配的Entity</returns>
		public Entity[] FindEntitiesByType(string type)
		{
			return Entities.Values.Where((entity) => { if (entity.EntityType == type) return true; return false; }).ToArray();
		}
		/// <summary>
		/// 通过玩家UID和Entity的类型查找Entity
		/// </summary>
		/// <param name="uid">UID</param>
		/// <param name="type">Entity Type</param>
		/// <returns>所有匹配的Entity</returns>
		public Entity FindEntitiesByUIDAndType(string uid, string type)
		{
			return Entities[(uid, type)];
		}
		/// <summary>
		/// 销毁对象
		/// </summary>
		public void Dispose()
		{
			foreach (Entity entity in Entities.Values)
			{
				entity.Dispose();
			}
		}
	}
}
