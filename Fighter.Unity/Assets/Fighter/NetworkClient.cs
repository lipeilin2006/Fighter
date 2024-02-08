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
	//�߹��kcp�ͻ���
	public static KcpClient Client { get; set; } = null;
	//�������UID
	public static string UID { get; set; }
	//������ҵ�Token
	public static string Token { get; set; }
	//��������Ƿ�ֹͣ
	static bool isStop = true;
	/// <summary>
	/// ����
	/// </summary>
	/// <param name="address">��ַ</param>
	/// <param name="port">�˿�</param>
	public static void Connect(string address,ushort port)
	{
		//����Ҫ��������һ��
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
		//��ʽ����
		Client.Connect(address, port);
		//�ı�״̬
		isStop = false;
		//ѭ�����¿ͻ���״̬����������һ��
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
	/// ����ָ���������ݣ�֮��Ϊ��֧��һ����ҿ��ƶ����Ϸ����Ҫ����EntityType����
	/// </summary>
	/// <param name="type">���ݰ�����</param>
	/// <param name="data">����</param>
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
	/// ͣ
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
