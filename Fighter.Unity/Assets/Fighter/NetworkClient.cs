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

public static class NetworkClient
{
	//高贵的kcp客户端
	public static KcpClient Client { get; set; } = null;
	//本地玩家UID
	public static string UID { get; set; }
	//本地玩家的Token
	public static string Token { get; set; }
	//用来标记是否停止
	static bool isStop = true;
	/// <summary>
	/// 连接
	/// </summary>
	/// <param name="address">地址</param>
	/// <param name="port">端口</param>
	public static void Connect(string address,ushort port)
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
	public static void Send(string type, byte[] data)
	{
		DataPack pack = new DataPack();
		pack.UID = UID;
		pack.type = type;
		pack.Token = Token;
		pack.data = data;
		BrotliCompressor compressor = new(System.IO.Compression.CompressionLevel.Fastest);
		MemoryPackSerializer.Serialize(compressor, pack);
		Client?.Send(compressor.ToArray(), KcpChannel.Reliable);
		Debug.Log($"Sended data:[{string.Join(',', compressor.ToArray())}]");
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
		Debug.Log($"Data:[{string.Join(',', data)},Channel:{channel}]");
	}
	static void OnDisconnected()
	{
		Debug.Log($"Disconnected");
	}
	static void OnError(ErrorCode error, string msg)
	{
		Debug.Log($"Error");
	}
}
