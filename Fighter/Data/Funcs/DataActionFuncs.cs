using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Funcs
{
    public static class DataActionFuncs
    {
        public static void Transform(int netid,string uid, string entityType, object? obj)
        {
            if (obj == null) return;
            TransformData td = (TransformData)obj;
            Console.WriteLine($"pos:({td.pos_x},{td.pos_y},{td.pos_z})");
        }
		public static void JoinRootRequest(int netid,string uid, string entityType, object? obj)
		{
			if (obj == null) return;
			JoinRootRequestData jrrd = (JoinRootRequestData)obj;
            Console.WriteLine($"UID:{uid} Request to join root {jrrd.rootID}");
		}
	}
}
