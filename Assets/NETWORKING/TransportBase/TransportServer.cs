using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

namespace TransportExample
{
    public enum GameEvent
	{
        NUMBER          = 0,
        NUMBER_REPLY    = 1,
	}

    delegate void GameEventHandler(DataStreamReader stream, object sender, NetworkConnection connection);

    public class TransportServer : MonoBehaviour
    {
        static Dictionary<GameEvent, GameEventHandler> gameEventDictionary = new Dictionary<GameEvent, GameEventHandler>() {
            // link game events to functions...
            { GameEvent.NUMBER, NumberHandler },
        };

        public NetworkDriver m_Driver;
        private NativeList<NetworkConnection> m_Connections;

        void Start() {
            m_Driver = NetworkDriver.Create();
            var endpoint = NetworkEndPoint.AnyIpv4;
            endpoint.Port = 1511;
            if (m_Driver.Bind(endpoint) != 0)
                Debug.Log("Failed to bind to port 1511");
            else
                m_Driver.Listen();

            m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        }

		private void OnDestroy() {
            m_Driver.Dispose();
            m_Connections.Dispose();
        }

        void Update() {
            m_Driver.ScheduleUpdate().Complete();

            // Clean up connections
            for (int i = 0; i < m_Connections.Length; i++) {
                if (!m_Connections[i].IsCreated) {
                    m_Connections.RemoveAtSwapBack(i);
                    --i;
                }
            }

            // Accept new connections
            NetworkConnection c;
            while ((c = m_Driver.Accept()) != default(NetworkConnection)) {
                m_Connections.Add(c);
                Debug.Log("Accepted a connection");
            }

            DataStreamReader stream;
            for (int i = 0; i < m_Connections.Length; i++) {
                if (!m_Connections[i].IsCreated)
                    continue;

                NetworkEvent.Type cmd;
                while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty) {
                    if (cmd == NetworkEvent.Type.Data) {
                        // Check which GameEvent we've received
                        GameEvent gameEventType = (GameEvent)stream.ReadUInt();
                        Debug.Log(gameEventType);

                        if ( gameEventDictionary.ContainsKey(gameEventType)) {
                            gameEventDictionary[gameEventType].Invoke(stream, this, m_Connections[i]);
						}
                        else {
                            //Unsupported event received...
						}                     
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect) {
                        Debug.Log("Client disconnected from server");
                        m_Connections[i] = default(NetworkConnection);
                    }
                }
            }
        }

        // Event Functions
        static void NumberHandler(DataStreamReader stream, object sender, NetworkConnection connection) {
            uint number = stream.ReadUInt();
            Debug.Log("Got " + number + " from the Client adding + 2 to it.");

            number += 2;

            TransportServer server = sender as TransportServer;

            DataStreamWriter writer;
            int result = server.m_Driver.BeginSend(NetworkPipeline.Null, connection, out writer);

            if (result == 0) {
                // Add GameEvent Reply uint
                writer.WriteUInt((uint)GameEvent.NUMBER_REPLY);

                writer.WriteUInt(number);
                server.m_Driver.EndSend(writer);
            }
        }
    }
}