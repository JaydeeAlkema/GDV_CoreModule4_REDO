using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using UnityEngine.UI;
using Unity.Networking.Transport.Utilities;
using UnityEngine.SceneManagement;

namespace ChatClientExample
{
    public class Client : MonoBehaviour
    {
        static Dictionary<NetworkMessageType, ClientMessageHandler> networkMessageHandlers = new Dictionary<NetworkMessageType, ClientMessageHandler> {
            { NetworkMessageType.HANDSHAKE_RESPONSE,        HandleServerHandshakeResponse },
            { NetworkMessageType.NETWORK_SPAWN,             HandleNetworkSpawn },             // uint networkId, uint objectType
            { NetworkMessageType.NETWORK_DESTROY,           HandleNetworkDestroy },           // uint networkId
            { NetworkMessageType.NETWORK_UPDATE_POSITION,   HandleNetworkUpdate },            // uint networkId, vector3 position, vector3 rotation
            { NetworkMessageType.CHAT_MESSAGE,              HandleChatMessage },
            { NetworkMessageType.PING,                      HandlePing }
        };

        public NetworkDriver m_Driver;
        public NetworkPipeline m_Pipeline;
        public NetworkConnection m_Connection;
        public bool Done;
        public NetworkManager networkManager;

        public ChatCanvas chatCanvas;

        public static string serverIP;
        public static string clientName = "";

        bool connected = false;
        float startTime = 0;

        // Start is called before the first frame update
        void Start() {
            startTime = Time.time;
            // Create connection to server IP
            m_Driver = NetworkDriver.Create(new ReliableUtility.Parameters { WindowSize = 32 });
            m_Pipeline = m_Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));

            m_Connection = default(NetworkConnection);

            var endpoint = NetworkEndPoint.Parse(serverIP, 9000, NetworkFamily.Ipv4);
            endpoint.Port = 1511;
            m_Connection = m_Driver.Connect(endpoint);
        }

        // No collections list this time...
        void OnApplicationQuit() {
            // Disconnecting on application exit currently (to keep it simple)
            if (m_Connection.IsCreated) {
                m_Connection.Disconnect(m_Driver);
                m_Connection = default(NetworkConnection);
            }
        }

        void OnDestroy() {
            m_Driver.Dispose();
        }

        void Update() {
            m_Driver.ScheduleUpdate().Complete();

            if (!connected && Time.time - startTime > 5f) {
                SceneManager.LoadScene(0);
            }

            if (!m_Connection.IsCreated) {
                if (!Done)
                    Debug.Log("Something went wrong during connect");
                return;
            }

            DataStreamReader stream;
            NetworkEvent.Type cmd;
            while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty) {
                if (cmd == NetworkEvent.Type.Connect) {
                    connected = true;
                    Debug.Log("We are now connected to the server");

                    // TODO: Create handshake message
                    var header = new HandshakeMessage {
                        name = clientName
                    };
                    SendPackedMessage(header);
                }
                else if (cmd == NetworkEvent.Type.Data) {
                    Done = true;

                    // First UInt is always message type (this is our own first design choice)
                    NetworkMessageType msgType = (NetworkMessageType)stream.ReadUShort();

                    // TODO: Create message instance, and parse data...
                    MessageHeader header = (MessageHeader)System.Activator.CreateInstance(NetworkMessageInfo.TypeMap[msgType]);
                    header.DeserializeObject(ref stream);

                    if (networkMessageHandlers.ContainsKey(msgType)) {
                        networkMessageHandlers[msgType].Invoke(this, header);
                    }
                    else {
                        Debug.LogWarning($"Unsupported message type received: {msgType}", this);
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect) {
                    Debug.Log("Client got disconnected from server");
                    m_Connection = default(NetworkConnection);
                }
            }
        }

        public InputField input;

        // UI FUNCTIONS (0 refs)
        public void SendMessage() {
            ChatMessage chatMsg = new ChatMessage {
                message = input.text
            };
            if ( connected ) SendPackedMessage(chatMsg);
            input.text = "";
        }

        public void ExitChat() {
            ChatQuitMessage chatQuitMsg = new ChatQuitMessage();
            if (connected) SendPackedMessage(chatQuitMsg);
            SceneManager.LoadScene(0);
        }
        // END UI FUNCTIONS

        public void SendPackedMessage( MessageHeader header ) {
            DataStreamWriter writer;
            int result = m_Driver.BeginSend(m_Pipeline, m_Connection, out writer);

            // non-0 is an error code
            if (result == 0) {
                header.SerializeObject(ref writer);
                m_Driver.EndSend(writer);
            }
            else {
                Debug.LogError($"Could not wrote message to driver: {result}", this);
            }
        }

        // Receive message function
        // TODO: rewrite as MessageHeader handlers
        //      - Server response handshake (DONE)
        //      - network spawn             (WIP)
        //      - network destroy           (WIP)
        //      - network update            (WIP)

        static void HandleServerHandshakeResponse(Client client, MessageHeader header) {
            HandshakeResponseMessage response = header as HandshakeResponseMessage;

            GameObject obj;
            if (client.networkManager.SpawnWithId(NetworkSpawnObject.PLAYER, response.networkId, out obj)) {
                NetworkedPlayer player = obj.GetComponent<NetworkedPlayer>();
                player.isLocal = true;
                player.isServer = false;
            }
            else {
                Debug.LogError("Could not spawn player!");
            }
        }

        static void HandleNetworkSpawn(Client client, MessageHeader header) {
            SpawnMessage spawnMsg = header as SpawnMessage;

            GameObject obj;
            if (!client.networkManager.SpawnWithId(spawnMsg.objectType, spawnMsg.networkId, out obj)) {
                Debug.LogError($"Could not spawn {spawnMsg.objectType} for id {spawnMsg.networkId}!");
            }
        }

        static void HandleNetworkDestroy(Client client, MessageHeader header) {
            DestroyMessage destroyMsg = header as DestroyMessage;
            if (!client.networkManager.DestroyWithId(destroyMsg.networkId)) {
                Debug.LogError($"Could not destroy object with id {destroyMsg.networkId}!");
            }
        }

        static void HandleNetworkUpdate(Client client, MessageHeader header) {
            UpdatePositionMessage posMsg = header as UpdatePositionMessage;

            GameObject obj;
            if (client.networkManager.GetReference(posMsg.networkId, out obj)) {
                obj.transform.position = posMsg.position;
                obj.transform.eulerAngles = posMsg.rotation;
            }
            else {
                Debug.LogError($"Could not find object with id {posMsg.networkId}!");
            }
        }

        static void HandleChatMessage(Client client, MessageHeader header) {
            ChatMessage chatMsg = header as ChatMessage;

            Color c = ChatCanvas.chatColor;
            if (chatMsg.messageType == MessageType.JOIN) c = ChatCanvas.joinColor;
            if (chatMsg.messageType == MessageType.QUIT) c = ChatCanvas.leaveColor;

            client.chatCanvas.NewMessage(chatMsg.message, c);
        }

        static void HandlePing(Client client, MessageHeader header) {
            Debug.Log("PING");

            PongMessage pongMsg = new PongMessage();
            client.SendPackedMessage(pongMsg);
        }
    }
}