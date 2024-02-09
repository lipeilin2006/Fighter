﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Fighter.Server
{
	//为了不增大网络包体,使用位反转加密,加密和解密其实是一样的
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
