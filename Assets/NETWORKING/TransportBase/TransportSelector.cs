using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TransportExample
{
    public class TransportSelector : MonoBehaviour
    {
        public void Client() {
            SceneManager.LoadScene("transport-client");
		}

        public void Server() {
            SceneManager.LoadScene("transport-server");
        }
    }
}