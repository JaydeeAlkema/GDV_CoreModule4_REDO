using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class NetWelcome : NetMessage
{
    public int AssignedTeam { get; set; }

    public NetWelcome()
    {
        code = OpCode.WELCOME;
    }
    public NetWelcome(DataStreamReader dataStreamReader)
    {
        code = OpCode.WELCOME;
        Deserialize(dataStreamReader);
    }

    public override void Serialize(ref DataStreamWriter dataStreamWriter)
    {
        dataStreamWriter.WriteByte((byte)code);
        dataStreamWriter.WriteInt(AssignedTeam);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        AssignedTeam = reader.ReadInt();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_WELCOME?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection connection)
    {
        NetUtility.S_WELCOME?.Invoke(this, connection);
    }
}
