using System;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{
    #region Singleton Implementation
    public static Client Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public NetworkDriver driver;

    private NetworkConnection connection;
    private bool isActive = false;

    public Action connectionDropped;

    #region Methods
    public void Update()
    {
        if (!isActive) return;

        driver.ScheduleUpdate().Complete();
        CheckAlive();
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

    public void Init(string ip, ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endPoint = NetworkEndPoint.Parse(ip, port);

        connection = driver.Connect(endPoint);

        Debug.Log($"Attempting to connect to Server on {endPoint.Address}");

        isActive = true;

        RegisterToEvent();
    }
    public void Shutdown()
    {
        if (isActive)
        {
            UnregisterToEvent();
            connection.Disconnect(driver);
            driver.Dispose();
            isActive = false;
        }
    }
    public void SendToServer(NetMessage msg)
    {
        DataStreamWriter streamWriter;
        driver.BeginSend(connection, out streamWriter);
        msg.Serialize(ref streamWriter);
        driver.EndSend(streamWriter);
    }

    /// <summary>
    /// Check if our connection to the server is alive or not.
    /// </summary>
    private void CheckAlive()
    {
        if (!connection.IsCreated && isActive)
        {
            Debug.Log("Something went wrong, lost connection to the server!");
            connectionDropped?.Invoke();
            Shutdown();
        }
    }
    /// <summary>
    /// Check for messages and handle them accordingly.
    /// </summary>
    private void UpdateMessagePump()
    {
        DataStreamReader streamReader;
        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(driver, out streamReader)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                SendToServer(new NetWelcome());
                Debug.Log("We're connected!");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                NetUtility.OnData(streamReader, default(NetworkConnection));
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                connection.Disconnect(driver);
                connectionDropped?.Invoke();
                Shutdown();
            }
        }
    }
    #endregion

    #region Event Parsing
    private void RegisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE += OnKeepAlive;
    }
    private void UnregisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
    }
    private void OnKeepAlive(NetMessage msg)
    {
        // Send it back, to keep both sides alive.
        SendToServer(msg);
    }
    #endregion
}
