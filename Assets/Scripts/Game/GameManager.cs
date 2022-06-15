using System.Collections.Generic;
using TMPro;
using Unity.Networking.Transport;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    [SerializeField] private GameObject MainMenuUI = default;
    [SerializeField] private TextMeshProUGUI currentTeamTurnText = default;
    [SerializeField] private TextMeshProUGUI currentTeamText = default;
    [Space]
    [SerializeField] private List<GameObject> UIPanels = new List<GameObject>();

    private byte playerCount = 255;
    private byte currentTeam = 255;
    private byte currentTeamTurn = 255;
    private byte gameState = 0;

    public static GameManager Instance { get => instance; private set => instance = value; }

    private void Awake()
    {
        if (instance == null || instance != this)
        {
            Destroy(instance);
            instance = this;
        }

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
        if (gameState == 0 && currentTeamTurn != currentTeam) return;

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

        MainMenuUI.SetActive(false);
    }
    private void OnPlayerInteractClient(NetMessage msg)
    {
        NetPlayerInteract netPlayerInteract = msg as NetPlayerInteract;

        currentTeamTurn = netPlayerInteract.TeamTurn;
        currentTeamTurnText.text = "Current Team Turn: " + currentTeamTurn;

        GameObject tileToDestroy = GameObject.Find(netPlayerInteract.TileName);

        if (tileToDestroy != null) Destroy(tileToDestroy);
    }
    #endregion
}
