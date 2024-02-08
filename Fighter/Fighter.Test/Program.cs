using Data;
using Fighter.Server;


FighterServer server = new FighterServer();
server.DeserializeFuncs.TryAdd("Transform", TransformData.Deserlize);
server.DataActions.TryAdd("Transform", TransformAction);
server.Start();

while (true)
{
	//这里可以写一些控制台指令，反正能让服务端一直运行就好
}

void TransformAction(object obj)
{
	TransformData td = (TransformData)obj;
	Console.WriteLine($"pos:({td.pos_x},{td.pos_y},{td.pos_z})");
}