using Data;
using Fighter;
using Fighter.ECS;
using System.Reflection;


Game.Init();
Console.WriteLine(Game.Server?.DeserializeFuncs.ContainsKey("Transform"));
Console.WriteLine(Game.Server?.DataActions.ContainsKey("Transform"));
Game.CreateRoot(0, "");
while (true)
{
	//这里可以写一些控制台指令，反正能让服务端一直运行就好
}