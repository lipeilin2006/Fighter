using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Timers;
using UnityEngine;
using Fighter;

public class GameManager : MonoBehaviour, IDisposable
{
	public int NetworkTickInterval = 30;
	public ConcurrentDictionary<(string, string), Entity> Entities { get; set; } = new();
	System.Random random = new();
	Timer timer = new();
	/// <summary>
	/// 要禁止销毁这个对象，然后要开一个计时器，用于Network Update
	/// </summary>
	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		timer.Interval = NetworkTickInterval;
		timer.Elapsed += NetworkUpdate;
		timer.Enabled = true;
		timer.Start();
	}
	/// <summary>
	/// 用于帧同步调用Component中的Network Update
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void NetworkUpdate(object sender, ElapsedEventArgs e)
	{
		foreach (Entity entity in Entities.Values)
		{
			entity.OnNetworkUpdate();
		}
	}

	private void Start()
	{
		foreach (Entity entity in Entities.Values)
		{
			entity.OnStart();
		}
	}
	private void Update()
	{
		foreach (Entity entity in Entities.Values)
		{
			entity.OnUpdate();
		}
	}
	private void FixedUpdate()
	{
		foreach (Entity entity in Entities.Values)
		{
			entity.OnFixedUpdate();
		}
	}
	private void LateUpdate()
	{
		foreach (Entity entity in Entities.Values)
		{
			entity.OnLateUpdate();
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
	/// 把Entity绑定到一个GameObject上
	/// </summary>
	/// <param name="entityType"></param>
	/// <param name="gameObject"></param>
	/// <returns></returns>
	public Entity AddEntity(GameObject gameObject, string entityType, string uid = "")
	{

		if (Entities.ContainsKey((uid, entityType))) return null;

		Entity entity = new(uid, entityType, gameObject);
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
	public void RemoveEntity(string uid,string type)
	{
		Entities.TryRemove((uid, type), out _);
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
