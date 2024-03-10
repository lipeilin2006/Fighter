namespace Fighter.Init
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Starting Server");
			FighterServer server = new();
			server.Start();
			while (true) { }
		}
	}
}
