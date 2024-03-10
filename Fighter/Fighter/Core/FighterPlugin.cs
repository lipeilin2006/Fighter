using kcp2k;

namespace Fighter.Core
{
    public abstract class FighterPlugin
    {
        public abstract void OnConnected(int id);
        public abstract void OnData(int id, ArraySegment<byte> data, KcpChannel channel);
        public abstract void OnDisconnected(int id);
        public abstract void OnError(int id, ErrorCode error, string msg);
    }
}
