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
	/// Ҫ��ֹ�����������Ȼ��Ҫ��һ����ʱ��������Network Update
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
	/// ����֡ͬ������Component�е�Network Update
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
	/// �����������Ϸ�������ٶ���
	/// </summary>
	private void OnApplicationQuit()
	{
		Dispose();
	}
	/// <summary>
	/// ��Entity�󶨵�һ��GameObject��
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
	/// ͨ�����UIDģ������Entity
	/// </summary>
	/// <param name="uid">UID</param>
	/// <returns>����ƥ���Entity</returns>
	public Entity[] FindEntitiesByUID(string uid)
	{
		return Entities.Values.Where((entity) => { if (entity.UID == uid) return true; return false; }).ToArray();
	}
	/// <summary>
	/// ͨ��Entity������ģ������Entity
	/// </summary>
	/// <param name="type">Entity Type</param>
	/// <returns>����ƥ���Entity</returns>
	public Entity[] FindEntitiesByType(string type)
	{
		return Entities.Values.Where((entity) => { if (entity.EntityType == type) return true; return false; }).ToArray();
	}
	/// <summary>
	/// ͨ�����UID��Entity�����Ͳ���Entity
	/// </summary>
	/// <param name="uid">UID</param>
	/// <param name="type">Entity Type</param>
	/// <returns>����ƥ���Entity</returns>
	public Entity FindEntitiesByUIDAndType(string uid, string type)
	{
		return Entities[(uid, type)];
	}
	public void RemoveEntity(string uid,string type)
	{
		Entities.TryRemove((uid, type), out _);
	}
	/// <summary>
	/// ���ٶ���
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
