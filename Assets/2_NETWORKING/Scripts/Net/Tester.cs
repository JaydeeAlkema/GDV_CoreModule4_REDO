using UnityEngine;
using UnityEngine.UI;

public class Tester : MonoBehaviour
{
    public Server server;
    public Client client;

    public InputField adressInput;


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
