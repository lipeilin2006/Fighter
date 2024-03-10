using Fighter.Core;
using Fighter.Plugins;

namespace Fighter.Init
{
    internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Starting Server");
			FighterServer server = new();
			server.LoadPlugin<SimplePlugin>();
			server.Start();
			while (true) { }
		}
	}
}
