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
	/// Ҫ��ֹ�����������Ȼ��Ҫ��һ����ʱ��������Network Update
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
	/// ����֡ͬ������Component�е�Network Update
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
	/// �����������Ϸ�������ٶ���
	/// </summary>
	private void OnApplicationQuit()
	{
		Dispose();
	}
	/// <summary>
	/// EntityIDҪȫ��Ψһ������Ҫ�������������
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
	/// ��Entity�󶨵�һ��GameObject��
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
	/// ��Entity�󶨵�һ��GameObject�ϣ���ָ�����Entity�����Ǹ����
	/// </summary>
	/// <param name="uid">���UID��Ψһ</param>
	/// <param name="entityType"></param>
	/// <param name="gameObject"></param>
	/// <returns>EntityID������ͨ�������ȡEntity����</returns>
	public int AddEntity(string uid, string entityType, GameObject gameObject)
	{
		int entityID = GenerateEntityID();
		Entities.TryAdd(entityID, new Entity(uid, entityType, entityID, gameObject));
		return entityID;
	}
	/// <summary>
	/// ͨ��EntityID��ȡEntity����
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public Entity FindEntityByID(int id)
	{
		return Entities[id];
	}
	/// <summary>
	/// ͨ�����UIDģ������Entity
	/// </summary>
	/// <param name="uid"></param>
	/// <returns></returns>
	public Entity[] FindEntitiesByUID(string uid)
	{
		return Entities.Values.Where((entity) => { if (entity.UID == uid) return true; return false; }).ToArray();
	}
	/// <summary>
	/// ͨ��Entity������ģ������Entity
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public Entity[] FindEntitiesByType(string type)
	{
		return Entities.Values.Where((entity) => { if (entity.EntityType == type) return true; return false; }).ToArray();
	}
	/// <summary>
	/// ͨ�����UID��Entity������ģ������Entity
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
