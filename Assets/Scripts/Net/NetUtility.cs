using System;
using Unity.Networking.Transport;
using UnityEngine;

public enum OpCode
{
    WELCOME = 1,
    START_GAME = 2,
    KEEP_ALIVE = 3,
    PLACE_TOWER = 4,
    REMATCH = 5,
}

public static class NetUtility
{
    public static void OnData(DataStreamReader streamReader, NetworkConnection connection, Server server = null)
    {
        NetMessage msg = null;
        var opCode = (OpCode)streamReader.ReadByte();

        // Ik ben me bewust over het feit dat een Dictionary hiervoor misschien een beter idee is. Maar ik vind dit zelf overzichtelijker.
        switch (opCode)
        {
            case OpCode.WELCOME: msg = new NetWelcome(streamReader); break;
            case OpCode.START_GAME: msg = new NetStartGame(streamReader); break;
            case OpCode.KEEP_ALIVE: msg = new NetKeepAlive(streamReader); break;
            //case OpCode.PLACE_TOWER: msg = new NetPlaceTower(streamReader); break;
            //case OpCode.REMATCH: msg = new NetRematch(streamReader); break;
            default:
                Debug.LogError("Message received had no OpCode!");
                break;
        }

        if (server != null)
        {
            msg.ReceivedOnServer(connection);
        }
        else
        {
            msg.ReceivedOnClient();
        }
    }

    // Net Messages
    // Client Messages
    public static Action<NetMessage> C_WELCOME;
    public static Action<NetMessage> C_START_GAME;
    public static Action<NetMessage> C_KEEP_ALIVE;
    public static Action<NetMessage> C_PLACE_TOWER;
    public static Action<NetMessage> C_REMATCH;

    // Server Messages
    public static Action<NetMessage, NetworkConnection> S_WELCOME;
    public static Action<NetMessage, NetworkConnection> S_START_GAME;
    public static Action<NetMessage, NetworkConnection> S_KEEP_ALIVE;
    public static Action<NetMessage, NetworkConnection> S_PLACE_TOWER;
    public static Action<NetMessage, NetworkConnection> S_REMATCH;
}
