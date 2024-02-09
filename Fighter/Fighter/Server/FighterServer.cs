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
        /// 用于身份认证，arg1:UID,arg2:Token,output:是否通过认证。每次收到数据包都会认证，防止身份伪造。这里的Token是安全密钥，一般记录在数据库中，每次登录时生成新的密钥。
        /// 那等号后面这块东西是什么呢？就是无论怎样都通过验证，也就是默认不验证。
        /// </summary>
        public Func<string, string, bool> AuthCheck { get; set; } = (_, _) => { return true; };
		/// <summary>
		/// 收到某个类型的数据后，需要进行具体的操作。这个就是用来注册具体类型数据包的相应操作函数。
		/// Key:类型，对应datapack中的type。
		/// Value:需执行的操作，由于框架本身不知道数据包对应的具体类型，所以Action只能传递一个object，可以在Action中进行类型转换。
		/// Action: NetID,UID,EntityType,DataObject
		/// Game.Init()会通过反射获取DataDeserializationFuncs里面的所有函数并注册到这里。
		/// </summary>
		public ConcurrentDictionary<string, Action<int, string, string, object?>> DataActions { get; set; } = new();
        /// <summary>
        /// 框架本身不知道你这byte数组是什么玩意啊。就算datapack中有个string类型的type，也不能通过string推出type。
        /// 因此，这个和DataAction类似。
        /// Game.Init()会通过反射获取DataActionFuncs里面的所有函数并注册到这里。
        /// </summary>
        public ConcurrentDictionary<string, Func<byte[], object?>> DeserializeFuncs { get; set; } = new();
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
            //数据包对象
            DataPack pack = MemoryPackSerializer.Deserialize<DataPack>(data);

            //用于验证玩家连接是否合法，这里的Token是安全密钥，一般记录在数据库中，每次登录时生成新的密钥。
            if (!AuthCheck(pack.UID, pack.token))
            {
                server?.Disconnect(netid);//不合法肯定要断开连接啊。
                return;//退，退，退
            }
            //部分解密。
            pack.data = Cryption.Decrypt(pack.token, pack.data);

            //终于拿到最终的数据了，开始执行操作
            if (DeserializeFuncs.ContainsKey(pack.type) && DataActions.ContainsKey(pack.type))
            {
                DataActions[pack.type](netid, pack.UID, pack.entityType, DeserializeFuncs[pack.type](pack.data));
            }
        }

        void OnConnected(int id)
        {
            Console.WriteLine($"{id} Connected");
        }
        void OnData(int id, ArraySegment<byte> data, KcpChannel channel)
        {
            Console.WriteLine($"Recv ID:{id},Length:{data.Count},Channel:{channel}");
            Task.Run(() => ProcessDataPack(id, data.ToArray()));
        }
        void OnDisconnected(int id)
        {
            Console.WriteLine($"{id} Disconnected");
        }
        void OnError(int id, ErrorCode error, string msg)
        {
            Console.WriteLine($"{id} Error");
        }
    }
}
