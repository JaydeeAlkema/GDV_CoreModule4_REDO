using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using Unity.Networking.Transport.Utilities;

public class Client : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;

    public bool done = false;

    private void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 1511;
        m_Connection = m_Driver.Connect(endpoint);
    }

    private void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = m_Driver.PopEventForConnection(m_Connection, out stream)) != NetworkEvent.Type.Empty)
        {
            if (!m_Connection.IsCreated)
            {
                if (!done)
                {
                    Debug.Log("Something went wrong during connect!");
                    return;
                }
            }

            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("Client connected to server...");
            }

            //else if (cmd == NetworkEvent.Type.Disconnect)
            //{
            //    Debug.Log("Client got disconnected from server!");
            //    m_Connection = default(NetworkConnection);
            //}
            //else if (cmd == NetworkEvent.Type.Data)
            //{
            //    uint value = stream.ReadUInt();
            //    Debug.Log($"Got the value = {value} back from server");
            //    done = true;
            //    m_Connection.Disconnect(m_Driver);
            //    m_Connection = default(NetworkConnection);
            //}
        }
    }
}
