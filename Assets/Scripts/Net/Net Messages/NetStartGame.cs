using Unity.Networking.Transport;

public class NetStartGame : NetMessage
{
    public byte TeamTurn { get; set; }
    public byte GameState { get; set; }

    public NetStartGame()
    {
        code = OpCode.START_GAME;
    }
    public NetStartGame(DataStreamReader dataStreamReader)
    {
        code = OpCode.START_GAME;
        Deserialize(dataStreamReader);
    }

    public override void Serialize(ref DataStreamWriter dataStreamWriter)
    {
        dataStreamWriter.WriteByte((byte)code);
        dataStreamWriter.WriteByte(TeamTurn);
        dataStreamWriter.WriteByte(GameState);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        TeamTurn = reader.ReadByte();
        GameState = reader.ReadByte();
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_START_GAME?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection connection)
    {
        NetUtility.S_START_GAME?.Invoke(this, connection);
    }
}
