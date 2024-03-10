using Fighter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Data.Funcs
{
    public static class DataActionFuncs
    {
        public static void Transform(int rootID, string uid, string entityType, object obj)
        {
            if (obj == null) return;
            TransformData td = (TransformData)obj;
            Debug.Log($"pos:({td.pos_x},{td.pos_y},{td.pos_z})");
        }
        public static void JoinRootRequest(int rootID, string uid, string entityType, object obj)
        {
            if (obj == null) return;
            JoinRootRequestData jrrd = (JoinRootRequestData)obj;
			Debug.Log($"UID:{uid} Request to join root {jrrd.rootID}");
        }
		public static void JoinRootCallback(int rootID, string uid, string entityType, object obj)
		{
			if (obj == null) return;
			JoinRootCallbackData jrc = (JoinRootCallbackData)obj;
            if (jrc.status == 0)
            {
                NetworkClient.RootID = jrc.rootID;
                GameManager.ClearEntity();
            }
		}
	}
}
