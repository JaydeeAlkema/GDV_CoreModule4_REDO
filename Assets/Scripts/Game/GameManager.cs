using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuUI = default;

    private int playerCount = -1;
    private int currentTeam = -1;

    private void Awake()
    {
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        NetUtility.S_WELCOME += OnWelcomeServer;

        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGameClient;
    }
    private void UnRegisterEvents()
    {
        NetUtility.S_WELCOME -= OnWelcomeServer;

        NetUtility.C_WELCOME -= OnWelcomeClient;
    }

    // Server
    private void OnWelcomeServer(NetMessage msg, NetworkConnection connection)
    {
        // Client has connected, assign a team and return the message back to the client.
        NetWelcome netWelcome = msg as NetWelcome;

        // Assign team
        netWelcome.AssignedTeam = ++playerCount;

        // Return back to the client
        Server.Instance.SendToClient(connection, netWelcome);

        // "Lobby" is full, start the game.
        if (playerCount == 1)
        {
            Server.Instance.Broadcast(new NetStartGame());
        }
    }

    // Client
    private void OnWelcomeClient(NetMessage msg)
    {
        // Client has connected, assign a team and return the message back to the client.
        NetWelcome netWelcome = msg as NetWelcome;

        // Assign team
        currentTeam = netWelcome.AssignedTeam;

        Debug.Log($"My assigned team is {currentTeam}");
    }

    private void OnStartGameClient(NetMessage msg)
    {
        MainMenuUI.SetActive(false);
    }
}
