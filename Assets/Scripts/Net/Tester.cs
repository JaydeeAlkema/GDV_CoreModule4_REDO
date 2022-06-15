using TMPro;
using UnityEngine;

public class Tester : MonoBehaviour
{
    public Server server = default;
    public Client client = default;

    public TMP_InputField adressInput = default;
    public TextMeshProUGUI usernameText = default;

    public Animator IpUIAnimator = default;

    private void Awake()
    {
        string username = PlayerPrefs.GetString("username");
        if (string.IsNullOrEmpty(username) || usernameText == null)
        {
            Debug.Log("Username is either empty (User hasn't logged in yet), or the username text UI element is not set/broken");
            return;
        }

        usernameText.text = $"Logged in as {username}";
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
}
