using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

namespace ChatClientExample
{
    public class UpdatePositionMessage : MessageHeader
    {
		public override NetworkMessageType Type { 
			get {
				return NetworkMessageType.NETWORK_UPDATE_POSITION;
			}
		}

		public uint networkId;
		public Vector3 position, rotation;

		public override void SerializeObject(ref DataStreamWriter writer) {
			// very important to call this first
			base.SerializeObject(ref writer);

			writer.WriteUInt(networkId);
			writer.WriteFloat(position.x);
			writer.WriteFloat(position.y);
			writer.WriteFloat(position.z);
			writer.WriteFloat(rotation.x);
			writer.WriteFloat(rotation.y);
			writer.WriteFloat(rotation.z);
		}

		public override void DeserializeObject(ref DataStreamReader reader) {
			// very important to call this first
			base.DeserializeObject(ref reader);

			networkId = reader.ReadUInt();
			position.x = reader.ReadFloat();
			position.y = reader.ReadFloat();
			position.z = reader.ReadFloat();
			rotation.x = reader.ReadFloat();
			rotation.y = reader.ReadFloat();
			rotation.z = reader.ReadFloat();
		}
	}
}