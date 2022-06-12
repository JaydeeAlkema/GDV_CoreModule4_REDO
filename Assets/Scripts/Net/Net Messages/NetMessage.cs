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

    }
    public virtual void ReceivedOnServer(NetworkConnection connection)
    {

    }
}
