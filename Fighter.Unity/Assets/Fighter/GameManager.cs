using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Timers;
using UnityEngine;
using Fighter;

public class GameManager : MonoBehaviour, IDisposable
{
	public int NetworkTickInterval = 30;
	public ConcurrentDictionary<int, Entity> Entities { get; set; } = new();
	System.Random random = new();
	Timer timer = new();
	/// <summary>
	/// 要禁止销毁这个对象，然后要开一个计时器，用于Network Update
	/// </summary>
	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		timer.Interval = NetworkTickInterval;
		timer.Elapsed += NetworkTick;
		timer.Enabled = true;
		timer.Start();
	}
	/// <summary>
	/// 用于帧同步调用Component中的Network Update
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void NetworkTick(object sender, ElapsedEventArgs e)
	{
		foreach (Entity entity in Entities.Values)
		{
			if (entity.OnNetworkTick != null) entity.OnNetworkTick();
		}
	}

	private void Start()
	{
		foreach (Entity entity in Entities.Values)
		{
			if (entity.OnStart != null) entity.OnStart();
		}
	}
	private void Update()
	{
		foreach (Entity entity in Entities.Values)
		{
			if (entity.OnUpdate != null) entity.OnUpdate();
		}
	}
	private void FixedUpdate()
	{
		foreach (Entity entity in Entities.Values)
		{
			if (entity.OnFixedUpdate != null) entity.OnFixedUpdate();
		}
	}
	private void LateUpdate()
	{
		foreach (Entity entity in Entities.Values)
		{
			if (entity.OnLateUpdate != null) entity.OnLateUpdate();
		}
	}
	/// <summary>
	/// 常规操作，游戏结束销毁对象
	/// </summary>
	private void OnApplicationQuit()
	{
		Dispose();
	}
	/// <summary>
	/// EntityID要全局唯一，所以要用这个函数生成
	/// </summary>
	/// <returns></returns>
	int GenerateEntityID()
	{
		int entityID;
		while (true)
		{
			entityID = random.Next();
			if (!Entities.ContainsKey(entityID)) break;
		}
		return entityID;
	}
	/// <summary>
	/// 把Entity绑定到一个GameObject上
	/// </summary>
	/// <param name="entityType"></param>
	/// <param name="gameObject"></param>
	/// <returns></returns>
	public int AddEntity(string entityType, GameObject gameObject)
	{
		int entityID = GenerateEntityID();
		Entities.TryAdd(entityID, new Entity("", entityType, entityID, gameObject));
		return entityID;
	}
	/// <summary>
	/// 把Entity绑定到一个GameObject上，并指定这个Entity属于那个玩家
	/// </summary>
	/// <param name="uid">玩家UID，唯一</param>
	/// <param name="entityType"></param>
	/// <param name="gameObject"></param>
	/// <returns>EntityID，可以通过这个获取Entity对象</returns>
	public int AddEntity(string uid, string entityType, GameObject gameObject)
	{
		int entityID = GenerateEntityID();
		Entities.TryAdd(entityID, new Entity(uid, entityType, entityID, gameObject));
		return entityID;
	}
	/// <summary>
	/// 通过EntityID获取Entity对象
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public Entity FindEntityByID(int id)
	{
		return Entities[id];
	}
	/// <summary>
	/// 通过玩家UID模糊查找Entity
	/// </summary>
	/// <param name="uid"></param>
	/// <returns></returns>
	public Entity[] FindEntitiesByUID(string uid)
	{
		return Entities.Values.Where((entity) => { if (entity.UID == uid) return true; return false; }).ToArray();
	}
	/// <summary>
	/// 通过Entity的类型模糊查找Entity
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public Entity[] FindEntitiesByType(string type)
	{
		return Entities.Values.Where((entity) => { if (entity.EntityType == type) return true; return false; }).ToArray();
	}
	/// <summary>
	/// 通过玩家UID和Entity的类型模糊查找Entity
	/// </summary>
	/// <param name="uid"></param>
	/// <param name="type"></param>
	/// <returns></returns>
	public Entity[] FindEntitiesByUIDAndType(string uid, string type)
	{
		return Entities.Values
			.Where((entity) => { if (entity.UID == uid) return true; return false; })
			.Where((entity) => { if (entity.EntityType == type) return true; return false; })
			.ToArray();
	}
	/// <summary>
	/// 销毁对象
	/// </summary>
	public void Dispose()
	{
		foreach(Entity entity in Entities.Values)
		{
			entity.Dispose();
		}
		NetworkClient.Stop();
	}
}
