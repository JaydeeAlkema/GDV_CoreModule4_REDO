using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tester : MonoBehaviour
{
    public Server server = default;
    public Client client = default;

    public TMP_InputField adressInput = default;
    public TextMeshProUGUI usernameText = default;

    public Animator IpUIAnimator = default;

    private int playerCount = -1;
    private int currentTeam = -1;

    private void Awake()
    {
        usernameText.text = $"Logged in as {PlayerPrefs.GetString("username")}";
        RegisterEvents();
    }

    public void OnPlayButton()
    {
        IpUIAnimator.SetTrigger("Move In");
    }

    public void OnOnlineHostButton()
    {
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
    }
    public void OnOnlineConnectButton()
    {
        client.Init(adressInput.text, 8007);
    }

    private void RegisterEvents()
    {
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.C_WELCOME += OnWelcomeClient;
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
}
