using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MemoryPack;
using MemoryPack.Compression;
using Fighter;
using Data;

public class NetworkTransform : Fighter.Component
{
	public TransformData transformData = new();
	private byte[] SerializeTransform()
	{
		BrotliCompressor compressor = new BrotliCompressor(System.IO.Compression.CompressionLevel.Fastest);
		MemoryPackSerializer.Serialize(compressor, transformData);
		Debug.Log("Serialized");
		return compressor.ToArray();
	}
	public override void FixedUpdate()
	{
		if (Entity.UID == NetworkClient.UID)
		{
			transformData.pos_x = gameObject.transform.position.x;
			transformData.pos_y = gameObject.transform.position.y;
			transformData.pos_z = gameObject.transform.position.z;
			transformData.rot_x = gameObject.transform.eulerAngles.x;
			transformData.rot_y = gameObject.transform.eulerAngles.y;
			transformData.rot_z = gameObject.transform.eulerAngles.z;
			transformData.scale_x = gameObject.transform.localScale.x;
			transformData.scale_y = gameObject.transform.localScale.y;
			transformData.scale_z = gameObject.transform.localScale.z;
		}
		Debug.Log("Got Transform Information");
	}
	public override void NetworkUpdate()
	{
		if (Entity.UID == NetworkClient.UID)
		{
			Debug.Log("Transform Updating");
			NetworkClient.Send("Transform", SerializeTransform());
			Debug.Log("Transform Updated");
		}
	}
}