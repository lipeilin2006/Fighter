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

namespace Fighter
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
            Pack pack = MemoryPackSerializer.Deserialize<Pack>(data);

            //用于验证玩家连接是否合法，这里的Token是安全密钥，一般记录在数据库中，每次登录时生成新的密钥。
            if (!AuthCheck(pack.UID, pack.token))
            {
                server?.Disconnect(netid);//不合法肯定要断开连接啊。
                return;//退，退，退
            }
            //部分解密。
            pack.data = Cryption.Decrypt(pack.token, pack.data);

            Router.Process(netid, pack.UID, pack.route, pack.data);
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
