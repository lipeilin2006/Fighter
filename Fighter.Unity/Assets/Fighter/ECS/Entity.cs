using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Fighter
{
	//����������еķ�������Ӧ�û����������ʶ������಻��Ҫ�ġ�
	public class Entity : IDisposable
	{
		public Action OnStart { get; set; } = () => { };
		public Action OnUpdate { get; set; } = () => { };
		public Action OnLateUpdate { get; set; } = () => { };
		public Action OnFixedUpdate { get; set; } = () => { };
		public Action OnNetworkUpdate{ get; set; } = () => { };
		public ConcurrentDictionary<Type, Component> components { get; set; } = new();
		public string UID { get; private set; }
		public string EntityType { get; private set; }
		public GameObject gameObject { get; private set; } = null;

		public Entity(string uid, string entityType, GameObject obj)
		{
			UID = uid;
			EntityType = entityType;
			gameObject = obj;
		}
		public T AddComponent<T>() where T : Component, new()
		{
			T component = new();
			if (components.TryAdd(typeof(T), component))
			{
				component.Entity = this;
				component.gameObject = gameObject;

				//ע���¼�
				OnStart += component.Start;
				OnUpdate += component.Update;
				OnFixedUpdate += component.FixedUpdate;
				OnLateUpdate += component.LateUpdate;
				OnNetworkUpdate += component.NetworkUpdate;

				component.Init();
				return component;
			}
			return null;
		}
		public bool AddComponent<T>(Component component) where T : Component, new()
		{
			component.Entity = this;
			component.gameObject = gameObject;

			//ע���¼�
			OnStart += component.Start;
			OnUpdate += component.Update;
			OnFixedUpdate += component.FixedUpdate;
			OnLateUpdate += component.LateUpdate;
			OnNetworkUpdate += component.NetworkUpdate;

			return components.TryAdd(typeof(T), component);
		}
		public T GetComponent<T>() where T : Component
		{
			if (components.ContainsKey(typeof(T)))
			{
				return (T)components[typeof(T)];
			}
			return null;
		}
		public bool RemoveComponent<T>() where T : Component
		{
			Component component;
			if (components.TryRemove(typeof(T), out component))
			{
				//ȡ���¼�
				OnStart -= component.Start;
				OnUpdate -= component.Update;
				OnFixedUpdate -= component.FixedUpdate;
				OnLateUpdate -= component.LateUpdate;
				OnNetworkUpdate -= component.NetworkUpdate;

				component.Dispose();
				return true;
			}
			return false;
		}
		public bool RemoveComponent(Type type)
		{
			Component component;
			if (components.TryRemove(type, out component))
			{
				//ȡ���¼�
				OnStart -= component.Start;
				OnUpdate -= component.Update;
				OnFixedUpdate -= component.FixedUpdate;
				OnLateUpdate -= component.LateUpdate;
				OnNetworkUpdate -= component.NetworkUpdate;

				component.Dispose();
				return true;
			}
			return false;
		}
		/// <summary>
		/// ����
		/// </summary>
		public void Dispose()
		{
			foreach (Type type in components.Keys)
			{
				RemoveComponent(type);
			}
		}
	}
}
