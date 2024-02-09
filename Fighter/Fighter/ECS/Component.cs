using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fighter.ECS
{
	//这个类不需要改。
	public abstract class Component : IDisposable
	{
		public Entity? Entity { get; set; } = null;
		public virtual void Init() { }
		public virtual void NetworkUpdate() { }
		public virtual void Dispose() { }
	}
}
