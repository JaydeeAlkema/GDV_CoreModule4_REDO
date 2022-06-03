using Unity.Networking.Transport;

public class NetMessage
{
    public OpCode code { get; set; }

    public virtual void Serialize(ref DataStreamWriter dataStreamWriter)
    {
        dataStreamWriter.WriteByte((byte)code);
    }
    public virtual void Deserialize(DataStreamReader reader)
    {

    }

    public virtual void ReceivedOnClient()
    {
        NetUtility.C_WELCOME?.Invoke(this);
    }
    public virtual void ReceivedOnServer(NetworkConnection connection)
    {
        NetUtility.S_WELCOME?.Invoke(this, connection);
    }
}
