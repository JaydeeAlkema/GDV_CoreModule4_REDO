using Unity.Networking.Transport;

public class NetPlayerInteract : NetMessage
{
    public int TeamID { get; set; }
    public int TeamTurn { get; set; }
    public string TileName { get; set; }
    public string TileNameTopNeighbour { get; set; }
    public string TileNameBottomNeighbour { get; set; }
    public string TileNameLeftNeighbour { get; set; }
    public string TileNameRightNeighbour { get; set; }
    // Dit zou efficienter zijn als het in de vorm van een (native)array zou zijn.

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
        dataStreamWriter.WriteFixedString32(TileNameTopNeighbour);
        dataStreamWriter.WriteFixedString32(TileNameBottomNeighbour);
        dataStreamWriter.WriteFixedString32(TileNameLeftNeighbour);
        dataStreamWriter.WriteFixedString32(TileNameRightNeighbour);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        TeamID = reader.ReadInt();
        TeamTurn = reader.ReadInt();
        TileName = reader.ReadFixedString32().ToString();
        TileNameTopNeighbour = reader.ReadFixedString32().ToString();
        TileNameBottomNeighbour = reader.ReadFixedString32().ToString();
        TileNameLeftNeighbour = reader.ReadFixedString32().ToString();
        TileNameRightNeighbour = reader.ReadFixedString32().ToString();
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
