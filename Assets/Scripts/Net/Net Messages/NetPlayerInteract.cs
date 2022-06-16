using Unity.Networking.Transport;

public class NetPlayerInteract : NetMessage
{
    public int TeamID { get; set; }
    public int TeamTurn { get; set; }
    public string TileName { get; set; }

    public NetPlayerInteract() // Create
    {
        code = OpCode.PLAYER_INTERACT;
    }
    public NetPlayerInteract(DataStreamReader reader) // Receive
    {
        code = OpCode.PLAYER_INTERACT;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter dataStreamWriter)
    {
        dataStreamWriter.WriteByte((byte)code);
        dataStreamWriter.WriteInt(TeamID);
        dataStreamWriter.WriteInt(TeamTurn);
        dataStreamWriter.WriteFixedString32(TileName);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        TeamID = reader.ReadInt();
        TeamTurn = reader.ReadInt();
        TileName = reader.ReadFixedString32().ToString();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_PLAYER_INTERACT?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection connection)
    {
        NetUtility.S_PLAYER_INTERACT?.Invoke(this, connection);
    }
}
