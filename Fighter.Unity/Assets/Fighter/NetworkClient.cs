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
		/// �յ�ĳ�����͵����ݺ���Ҫ���о���Ĳ����������������ע������������ݰ�����Ӧ����������
		/// Key:���ͣ���Ӧdatapack�е�type��
		/// Value:��ִ�еĲ��������ڿ�ܱ���֪�����ݰ���Ӧ�ľ������ͣ�����Actionֻ�ܴ���һ��object��������Action�н�������ת����
		/// Action: RootID,UID,EntityType,DataObject,�ȷ������һ��NetID����
		/// </summary>
		public static ConcurrentDictionary<string, Action<int, string, string, object>> DataActions { get; set; } = new();
		/// <summary>
		/// ��ܱ���֪������byte������ʲô���Ⱑ������datapack���и�string���͵�type��Ҳ����ͨ��string�Ƴ�type��
		/// ��ˣ������DataAction���ơ��Լ�ע�ᷴ���л��������ɡ�
		/// </summary>
		public static ConcurrentDictionary<string, Func<byte[], object>> DeserializeFuncs { get; set; } = new();
		//�߹��kcp�ͻ���
		public static KcpClient Client { get; set; } = null;
		//�������UID
		public static string UID { get; set; }
		//������ҵ�Token
		public static string Token { get; set; }
		//RootID
		public static int RootID { get; set; } = 0;
		//��������Ƿ�ֹͣ
		static bool isStop = true;
		/// <summary>
		/// ����
		/// </summary>
		/// <param name="address">��ַ</param>
		/// <param name="port">�˿�</param>
		public static void Connect(string address, ushort port)
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
		public static void Send(string packtype, string entityType, byte[] data)
		{
			DataPack pack = new();
			pack.UID = UID;
			pack.entityType = entityType;
			pack.type = packtype;
			pack.token = Token;
			pack.data = Cryption.Encrypt(pack.token, data);//���ּ���

			Client?.Send(MemoryPackSerializer.Serialize(pack), KcpChannel.Reliable);
			Debug.Log("Sended");
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
		/// �յ����ݰ���ȻҪ����
		/// </summary>
		/// <param name="netid">ȫ��Ψһ����ID</param>
		/// <param name="data">�յ���byte����</param>
		static void ProcessDataPack(byte[] data)
		{
			//�������ݽ�����ѹ��������൱�ڽ�ѹ
			BrotliDecompressor decompressor = new();
			//���ݰ�����������ѹ
			DataPack pack = MemoryPackSerializer.Deserialize<DataPack>(decompressor.Decompress(data));
			//���ֽ��ܡ�
			pack.data = Cryption.Decrypt(pack.token, pack.data);

			//�����õ����յ������ˣ���ʼִ�в���
			if (DeserializeFuncs.ContainsKey(pack.type) && DataActions.ContainsKey(pack.type))
			{
				DataActions[pack.type](pack.rootID, pack.UID, pack.entityType, DeserializeFuncs[pack.type](pack.data));
			}
		}
	}
}
