using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ChatClientExample
{
    public class ClientServerSelection : MonoBehaviour
    {
        public string serverScene, clientScene;
        public InputField serverIPInput, nameInput;

		private void Start() {
            Application.targetFrameRate = 60;
		}

		public void GoServer() {
            SceneManager.LoadScene(serverScene);
        }

        public void GoClient() {
            NetworkEndPoint endPoint;
            if (NetworkEndPoint.TryParse(serverIPInput.text, 9000, out endPoint, NetworkFamily.Ipv4)) {
                Client.serverIP = serverIPInput.text;
            }
            else {
                Client.serverIP = "127.0.0.1";
            }

            string name = nameInput.text;
            if (string.IsNullOrEmpty(nameInput.text)) {
                name = "";
                for (int i = 0; i < 16; ++i) {
                    name += (char)Random.Range(97, 97 + 26);
                }
            }
            Client.clientName = name;

            SceneManager.LoadScene(clientScene);
        }
    }
}