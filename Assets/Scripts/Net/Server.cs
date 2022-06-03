// https://www.youtube.com/watch?v=lPoiTw0qjtc&list=PLmcbjnHce7SeAUFouc3X9zqXxiPbCz8Zp&index=11

using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Server : MonoBehaviour
{
    #region Singleton Implementation
    public static Server Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public NetworkDriver driver;

    private NativeList<NetworkConnection> connections;
    private bool isActive = false;
    private const float keepAliveTickRate = 20.0f;
    private float lastKeepAlive;

    public Action connectionDropped;

    #region Methods
    public void Update()
    {
        if (!isActive) return;

        KeepAlive();

        driver.ScheduleUpdate().Complete();

        CleanupConnections();
        AcceptNewConnections();
        UpdateMessagePump();
    }
    public void OnDestroy()
    {
        Shutdown();
    }
    public void OnApplicationQuit()
    {
        Shutdown();
    }

    public void Init(ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endPoint = NetworkEndPoint.AnyIpv4;
        endPoint.Port = port;

        // 0 is success
        if (driver.Bind(endPoint) != 0)
        {
            Debug.Log($"Unable to bind on port {endPoint.Port}");
            return;
        }
        else
        {
            driver.Listen();
            Debug.Log($"Currently listening on port {endPoint.Port}");
        }

        // Only ever allow maximum of 2 connections.
        connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
        isActive = true;
    }
    public void Shutdown()
    {
        if (isActive)
        {
            connections.Dispose();
            driver.Dispose();
            isActive = false;
        }
    }

    /// <summary>
    /// Keep the connection alive
    /// </summary>
    private void KeepAlive()
    {
        if (Time.time - lastKeepAlive > keepAliveTickRate)
        {
            lastKeepAlive = Time.time;
            Broadcast(new NetKeepAlive());
        }
    }
    /// <summary>
    /// Check if there are any people not connected, but still in the list of connections.
    /// </summary>
    private void CleanupConnections()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }
    }
    /// <summary>
    /// Is there anybody waiting to connect, if so, connect them if possible.
    /// </summary>
    private void AcceptNewConnections()
    {
        NetworkConnection c;
        while ((c = driver.Accept()) != default(NetworkConnection))
        {
            connections.Add(c);
        }
    }
    /// <summary>
    /// Check for messages and handle them accordingly.
    /// </summary>
    private void UpdateMessagePump()
    {
        DataStreamReader streamReader;
        for (int i = 0; i < connections.Length; i++)
        {
            NetworkEvent.Type cmd;
            while ((cmd = driver.PopEventForConnection(connections[i], out streamReader)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    NetUtility.OnData(streamReader, connections[i], this);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    connections[i].Disconnect(driver);
                    connectionDropped?.Invoke();
                    Shutdown(); // This is unusual, but for a 2 person game that required both people to be connected, it isn't unusual.
                }
            }
        }
    }
    #endregion

    #region Server Specific Methods
    /// <summary>
    /// Send specific message to a specific connection (user)
    /// </summary>
    /// <param name="connection"> Connection to send message to. </param>
    /// <param name="msg"> Message to send to connection to. </param>
    public void SendToClient(NetworkConnection connection, NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }
    /// <summary>
    /// Broadcast message to all connections (users).
    /// </summary>
    /// <param name="msg"> Message to send to all connections (users). </param>
    public void Broadcast(NetMessage msg)
    {
        for (int i = 0; i < connections.Length; i++)
        {
            Debug.Log($"Sending {msg.code} to : {connections[i].InternalId}");
            SendToClient(connections[i], msg);
        }
    }
    #endregion
}
