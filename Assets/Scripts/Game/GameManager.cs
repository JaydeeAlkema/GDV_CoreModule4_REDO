using NaughtyAttributes;
using System.Collections.Generic;
using TMPro;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    [Space]
    [BoxGroup("UI References"), SerializeField] private GameObject MainMenuUI = default;
    [BoxGroup("UI References"), SerializeField] private TextMeshProUGUI currentTeamTurnText = default;
    [BoxGroup("UI References"), SerializeField] private TextMeshProUGUI currentTeamText = default;
    [BoxGroup("UI References"), SerializeField] private TMP_InputField adressInput = default;
    [BoxGroup("UI References"), SerializeField] private TextMeshProUGUI usernameText = default;
    [BoxGroup("UI References"), SerializeField] private TextMeshProUGUI connectionStatusText = default;
    [BoxGroup("UI References"), SerializeField] private List<GameObject> UIPanels = new List<GameObject>();
    [Space]
    [BoxGroup("UI References"), SerializeField] private Button playGameButton = default;
    [BoxGroup("UI References"), SerializeField] private Button userLoginButton = default;
    [BoxGroup("UI References"), SerializeField] private Button editUserButton = default;

    [Space]
    [BoxGroup("Networked Variables"), SerializeField] private Server server = default;
    [BoxGroup("Networked Variables"), SerializeField] private Client client = default;
    [BoxGroup("Networked Variables"), SerializeField] private int playerCount = -1;
    [BoxGroup("Networked Variables"), SerializeField] private int currentTeam = -1;
    [BoxGroup("Networked Variables"), SerializeField] private int currentTeamTurn = 0;
    [BoxGroup("Networked Variables"), SerializeField] private int gameState = 0;

    [Space]
    [BoxGroup("Webconnectivity Variables"), SerializeField] private bool serverLoggedIn = false;
    [BoxGroup("Webconnectivity Variables"), SerializeField] private bool userLoggedIn = false;
    [BoxGroup("Webconnectivity Variables"), SerializeField] private UserData userData = new UserData();

    public static GameManager Instance { get => instance; private set => instance = value; }
    public bool ServerLoggedIn { get { return serverLoggedIn; } set { serverLoggedIn = value; userLoginButton.interactable = serverLoggedIn; playGameButton.interactable = serverLoggedIn; } }
    public bool UserLoggedIn { get { return userLoggedIn; } set { userLoggedIn = value; editUserButton.interactable = userLoggedIn; } }
    public UserData UserData { get => userData; set => userData = value; }

    private void Awake()
    {
        if (instance == null || instance != this) instance = this;
        if (userData == null) userData = new UserData();

        userLoginButton.interactable = false;
        editUserButton.interactable = false;
        playGameButton.interactable = false;

        string username = PlayerPrefs.GetString("username");
        if (string.IsNullOrEmpty(username) || usernameText == null)
        {
            Debug.Log("Username is either empty (User hasn't logged in yet), or the username text UI element is not set/broken");
            return;
        }

        usernameText.text = $"Logged in as {username}";

        RegisterEvents();
    }

    private void Update()
    {
        RaycastForGridTile();
    }

    #region Gameplay Stuff
    private void RaycastForGridTile()
    {
        // Only raycast if the gamestate is 0 (play mode) and it's currently the users turn.
        if (gameState == 0 || currentTeamTurn != currentTeam) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000, LayerMask.GetMask("Tile")))
        {
            if (Input.GetMouseButtonDown(0) == true && hit.collider.GetComponent<ISelectable>() != null)
            {
                NetPlayerInteract netPlayerInteract = new NetPlayerInteract();
                netPlayerInteract.TeamID = currentTeam;
                netPlayerInteract.TeamTurn = currentTeam == 1 ? (byte)0 : (byte)1;
                netPlayerInteract.TileName = hit.collider.GetComponent<ISelectable>()?.Select();
                Client.Instance.SendToServer(netPlayerInteract);
            }
        }
    }

    /// <summary>
    /// Toggles the UI panel with the given index.
    /// </summary>
    /// <param name="index"></param>
    public void ToggleUIPanel(int index)
    {
        for (int i = 0; i < UIPanels.Count; i++)
        {
            if (i == index)
            {
                UIPanels[i].SetActive(true);
            }
            else
            {
                UIPanels[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Disables all the UI panels (except ingame UI)
    /// </summary>
    public void DisableUI()
    {
        foreach (GameObject panel in UIPanels)
        {
            panel.SetActive(false);
        }
    }

    /// <summary>
    /// Quits the game...
    /// </summary>
    public void QuitGame()
    {
        PlayerPrefs.DeleteAll();
        Application.Quit();
    }
    #endregion

    #region Networking Stuff
    private void RegisterEvents()
    {
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.S_PLAYER_INTERACT += OnPlayerInteractServer;

        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGameClient;
        NetUtility.C_PLAYER_INTERACT += OnPlayerInteractClient;
    }
    private void UnRegisterEvents()
    {
        NetUtility.S_WELCOME -= OnWelcomeServer;
        NetUtility.S_PLAYER_INTERACT -= OnPlayerInteractServer;

        NetUtility.C_WELCOME -= OnWelcomeClient;
        NetUtility.C_START_GAME -= OnStartGameClient;
        NetUtility.C_PLAYER_INTERACT -= OnPlayerInteractClient;
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
            NetStartGame netStartGame = new NetStartGame();
            netStartGame.TeamTurn = (byte)Random.Range(0, 2);
            netStartGame.GameState = 1;

            Server.Instance.Broadcast(netStartGame);
        }
    }
    private void OnPlayerInteractServer(NetMessage msg, NetworkConnection connection)
    {
        NetPlayerInteract netPlayerInteract = msg as NetPlayerInteract;

        // Receive, and just broadcast it back.
        Server.Instance.Broadcast(netPlayerInteract);
    }

    // Client
    private void OnWelcomeClient(NetMessage msg)
    {
        // Client has connected, assign a team and return the message back to the client.
        NetWelcome netWelcome = msg as NetWelcome;

        // Assign team
        currentTeam = netWelcome.AssignedTeam;
        currentTeamText.text = "Team: " + currentTeam;
    }
    private void OnStartGameClient(NetMessage msg)
    {
        // Both clients are connected, the game can now start.
        NetStartGame netStartGame = msg as NetStartGame;

        currentTeamTurn = netStartGame.TeamTurn;
        gameState = netStartGame.GameState;

        currentTeamTurnText.text = "Current Team Turn: " + currentTeamTurn;

        DisableUI();
    }
    private void OnPlayerInteractClient(NetMessage msg)
    {
        NetPlayerInteract netPlayerInteract = msg as NetPlayerInteract;

        currentTeamTurn = netPlayerInteract.TeamTurn;
        currentTeamTurnText.text = "Current Team Turn: " + currentTeamTurn;

        GameObject tileToDestroy = GameObject.Find(netPlayerInteract.TileName);

        if (tileToDestroy != null) Destroy(tileToDestroy);
    }

    // UI Button Events
    public void OnOnlineHostButton()
    {
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
        connectionStatusText.text = "Waiting for other player to connect...";
    }
    public void OnOnlineConnectButton()
    {
        client.Init(adressInput.text, 8007);
        connectionStatusText.text = "Connecting...";
    }
    #endregion
}
