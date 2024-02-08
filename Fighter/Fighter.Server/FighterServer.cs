using Data;
using kcp2k;
using MemoryPack;
using MemoryPack.Compression;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Text;

namespace Fighter.Server
{
	public class FighterServer
	{
		public ushort Port { get; set; } = 20060;
		public int MaxClientCount { get; set; } = 10000;
		/// <summary>
		/// 记录所有连接的玩家。int:全局唯一网络ID，不用自己手动生成。
		/// </summary>
		public ConcurrentDictionary<int, Player> Players { get; private set; } = new();
		/// <summary>
		/// 用于身份认证，arg1:UID,arg2:Token,output:是否通过认证。每次收到数据包都会认证，防止身份伪造。这里的Token是安全密钥，一般记录在数据库中，每次登录时生成新的密钥。
		/// 那等号后面这块东西是什么呢？就是无论怎样都通过验证，也就是默认不验证。
		/// </summary>
		public Func<string, string, bool> AuthCheck { get; set; } = (_, _) => { return true; };
		/// <summary>
		/// 收到某个类型的数据后，需要进行具体的操作。这个就是用来注册具体类型数据包的相应操作函数。
		/// Key:类型，对应datapack中的type。
		/// Value:需执行的操作，由于框架本身不知道数据包对应的具体类型，所以Action只能传递一个object，可以在Action中进行类型转换。
		/// </summary>
		public ConcurrentDictionary<string, Action<object>> DataActions { get; set; } = new();
		/// <summary>
		/// 框架本身不知道你这byte数组是什么玩意啊。就算datapack中有个string类型的type，也不能通过string推出type。
		/// 因此，这个和DataAction类似。自己注册反序列化处理函数吧。
		/// </summary>
		public ConcurrentDictionary<string, Func<byte[], object>> DeserializeFuncs { get; set; } = new();
		/// <summary>
		/// 和上面的类似，序列化函数也要自己注册，别嫌麻烦。写不起就去用没有性能可言的python吧。
		/// </summary>
		public ConcurrentDictionary<string, Func<object, byte[]>> SerializeFuncs { get; set; } = new();
		/// <summary>
		/// 加密。空着就是不加密。推荐对称加密，省事。每次连接的时候发个密钥过去，密钥随机就好。
		/// </summary>
		public Func<Player, byte[], byte[]>? Encrypt { get; set; } = null;
		/// <summary>
		/// 解密。空着就是不解密。
		/// </summary>
		public Func<Player, byte[], byte[]>? Decrypt { get; set; } = null;
		/// <summary>
		/// 高贵的kcp服务器对象。
		/// </summary>
		KcpServer? server = null;

		/// <summary>
		/// Kcp服务器，启动！！！
		/// </summary>
		/// <exception cref="Exception"></exception>
		public void Start()
		{
			//参数很多，慢慢看
			server = new KcpServer(
				OnConnected,
				OnData,
				OnDisconnected,
				OnError,
				new KcpConfig(
					NoDelay: true,
					DualMode: false,
					Interval: 1,//刷新频率，下面的Tick函数里也要一致。
					Timeout: 2000,
					SendWindowSize: Kcp.WND_SND * 100,
					ReceiveWindowSize: Kcp.WND_RCV * 100,
					CongestionWindow: false,
					MaxRetransmits: Kcp.DEADLINK * 2
			));
			//这下才是真的启动
			server.Start(Port);
			//因为要
			Task.Run(Tick);
			Console.WriteLine("Started");
		}

		void Tick()
		{
			while (true)
			{
				Thread.Sleep(1);//刷新频率，跟上面要一致。
				server?.Tick();//kcp的特色。Tick越快，单位时间内发的越多
			}
		}
		/// <summary>
		/// 收到数据包当然要处理啊
		/// </summary>
		/// <param name="netid">全局唯一网络ID</param>
		/// <param name="data">收到的byte数组</param>
		void ProcessDataPack(int netid, byte[] data)
		{
			//传输数据进行了压缩，这个相当于解压
			BrotliDecompressor decompressor = new();
			//发这个数据包的玩家
			Player player;
			//数据包对象
			DataPack pack = MemoryPackSerializer.Deserialize<DataPack>(decompressor.Decompress(data));

			//用于验证玩家连接是否合法，这里的Token是安全密钥，一般记录在数据库中，每次登录时生成新的密钥。
			if (!AuthCheck(pack.UID, pack.Token))
			{
				server?.Disconnect(netid);//不合法肯定要断开连接啊。
				return;//退，退，退
			}
			//这个玩家之前究竟有没有连过呢
			if (Players.ContainsKey(netid))
			{
				//连过了，直接拿来
				player = Players[netid];
			}
			else
			{
				//没连过，创建新的
				player = new Player(pack.UID, pack.Token, netid);
				Players.TryAdd(netid, player);
			}
			//能解密就解密。
			if (Decrypt != null) data = Decrypt(player, data);

			//终于拿到最终的数据了，开始执行操作
			if (DeserializeFuncs.ContainsKey(pack.type) && DataActions.ContainsKey(pack.type))
			{
				DataActions[pack.type](DeserializeFuncs[pack.type](pack.data));
			}
		}

		void OnConnected(int id)
		{
			Console.WriteLine($"{id} Connected");
		}
		void OnData(int id,ArraySegment<byte> data,KcpChannel channel)
		{
			Console.WriteLine($"Recv ID:{id},Length:{data.Count},Channel:{channel}");
			Task.Run(() => ProcessDataPack(id, data.ToArray()));
		}
		void OnDisconnected(int id)
		{
			Console.WriteLine($"{id} Disconnected");
		}
		void OnError(int id,ErrorCode error,string msg)
		{
			Console.WriteLine($"{id} Error");
		}
	}
}
