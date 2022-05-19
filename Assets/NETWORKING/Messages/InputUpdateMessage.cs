using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

namespace ChatClientExample
{
    public class InputUpdateMessage : MessageHeader
    {
		public override NetworkMessageType Type { 
			get {
				return NetworkMessageType.INPUT_UPDATE;
			}
		}

		public InputUpdate input;
		public uint networkId;

		public override void SerializeObject(ref DataStreamWriter writer) {
			// very important to call this first
			base.SerializeObject(ref writer);

			writer.WriteUInt(networkId);
			writer.WriteFloat(input.horizontal);
			writer.WriteFloat(input.vertical);
			writer.WriteUShort((ushort) ( input.fire ? 1 : 0 ) );
			writer.WriteUShort((ushort) ( input.jump ? 1 : 0 ) );
		}

		public override void DeserializeObject(ref DataStreamReader reader) {
			// very important to call this first
			base.DeserializeObject(ref reader);

			networkId = reader.ReadUInt();
			input.horizontal = reader.ReadFloat();
			input.vertical = reader.ReadFloat();
			input.fire = reader.ReadUShort() != 0;
			input.jump = reader.ReadUShort() != 0;
		}
	}
}