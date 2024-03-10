using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using kcp2k;
using System;
using System.Threading;
using MemoryPack;
using MemoryPack.Compression;
using System.Threading.Tasks;
using Data;
using Fighter;
using System.Collections.Concurrent;

namespace Fighter
{
	public static class NetworkClient
	{
		/// <summary>
		/// 收到某个类型的数据后，需要进行具体的操作。这个就是用来注册具体类型数据包的相应操作函数。
		/// Key:类型，对应datapack中的type。
		/// Value:需执行的操作，由于框架本身不知道数据包对应的具体类型，所以Action只能传递一个object，可以在Action中进行类型转换。
		/// Action: RootID,UID,EntityType,DataObject,比服务端少一个NetID参数
		/// </summary>
		public static ConcurrentDictionary<string, Action<int, string, string, object>> DataActions { get; set; } = new();
		/// <summary>
		/// 框架本身不知道你这byte数组是什么玩意啊。就算datapack中有个string类型的type，也不能通过string推出type。
		/// 因此，这个和DataAction类似。自己注册反序列化处理函数吧。
		/// </summary>
		public static ConcurrentDictionary<string, Func<byte[], object>> DeserializeFuncs { get; set; } = new();
		//高贵的kcp客户端
		public static KcpClient Client { get; set; } = null;
		//本地玩家UID
		public static string UID { get; set; }
		//本地玩家的Token
		public static string Token { get; set; }
		//RootID
		public static int RootID { get; set; } = 0;
		//用来标记是否停止
		static bool isStop = true;
		/// <summary>
		/// 连接
		/// </summary>
		/// <param name="address">地址</param>
		/// <param name="port">端口</param>
		public static void Connect(string address, ushort port)
		{
			//参数要跟服务器一样
			Client = new KcpClient(
				OnConnected,
				OnData,
				OnDisconnected,
				OnError,
				new KcpConfig(
					NoDelay: true,
					DualMode: false,
					Interval: 1,
					Timeout: 2000,
					SendWindowSize: Kcp.WND_SND * 100,
					ReceiveWindowSize: Kcp.WND_RCV * 100,
					CongestionWindow: false,
					MaxRetransmits: Kcp.DEADLINK * 2
				));
			//正式连接
			Client.Connect(address, port);
			//改变状态
			isStop = false;
			//循环更新客户端状态，跟服务器一样
			Task.Run(Tick);
		}
		static void Tick()
		{
			while (!isStop)
			{
				Thread.Sleep(1);
				Client?.Tick();
			}
		}
		/// <summary>
		/// 发送指定类型数据，之后为了支持一个玩家控制多个游戏对象，要加入EntityType参数
		/// </summary>
		/// <param name="type">数据包类型</param>
		/// <param name="data">内容</param>
		public static void Send(string packtype, string entityType, byte[] data)
		{
			DataPack pack = new();
			pack.UID = UID;
			pack.entityType = entityType;
			pack.type = packtype;
			pack.token = Token;
			pack.data = Cryption.Encrypt(pack.token, data);//部分加密

			Client?.Send(MemoryPackSerializer.Serialize(pack), KcpChannel.Reliable);
			Debug.Log("Sended");
		}
		/// <summary>
		/// 停
		/// </summary>
		public static void Stop()
		{
			isStop = true;
			Client?.Disconnect();
			Client = null;
		}
		static void OnConnected()
		{
			Debug.Log($"Connected");
		}
		static void OnData(ArraySegment<byte> data, KcpChannel channel)
		{
			Debug.Log($"Recv Data,Length{data.Count},Channel:{channel}");
			ProcessDataPack(data.ToArray());
		}
		static void OnDisconnected()
		{
			Debug.Log($"Disconnected");
		}
		static void OnError(ErrorCode error, string msg)
		{
			Debug.Log($"Error");
		}

		/// <summary>
		/// 收到数据包当然要处理啊
		/// </summary>
		/// <param name="netid">全局唯一网络ID</param>
		/// <param name="data">收到的byte数组</param>
		static void ProcessDataPack(byte[] data)
		{
			//传输数据进行了压缩，这个相当于解压
			BrotliDecompressor decompressor = new();
			//数据包对象，整包解压
			DataPack pack = MemoryPackSerializer.Deserialize<DataPack>(decompressor.Decompress(data));
			//部分解密。
			pack.data = Cryption.Decrypt(pack.token, pack.data);

			//终于拿到最终的数据了，开始执行操作
			if (DeserializeFuncs.ContainsKey(pack.type) && DataActions.ContainsKey(pack.type))
			{
				DataActions[pack.type](pack.rootID, pack.UID, pack.entityType, DeserializeFuncs[pack.type](pack.data));
			}
		}
	}
}
