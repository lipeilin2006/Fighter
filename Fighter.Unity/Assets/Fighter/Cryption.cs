using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Fighter
{
    public class Cryption
    {
		public static byte[] Encrypt(string token, byte[] data)
		{
            byte[] key = Encoding.ASCII.GetBytes(token);
            int keyIndex = 0;
			byte[] encrypted = new byte[data.Length];
			for (int i = 0; i < data.Length; i++)
            {
                encrypted[i] = (byte)(data[i] ^ key[keyIndex]);
                keyIndex++;
                if (keyIndex >= key.Length) keyIndex = 0;
            }
            return encrypted;
		}
        public static byte[] Decrypt(string token, byte[] data)
        {
			byte[] key = Encoding.ASCII.GetBytes(token);
			int keyIndex = 0;
			byte[] encrypted = new byte[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				encrypted[i] = (byte)(data[i] ^ key[keyIndex]);
				keyIndex++;
				if (keyIndex >= key.Length) keyIndex = 0;
			}
			return encrypted;
		}
    }
}
