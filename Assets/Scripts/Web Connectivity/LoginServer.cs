using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class LoginServer : MonoBehaviour
{
    #region Privates
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField serverNameTextInputField = default;
    [SerializeField] private TMP_InputField passwordTextInputfield = default;
    [Space]
    [SerializeField] private TMP_Text userFeedbackMessageText = default;
    [SerializeField] private int uiPanelToToggleOnSuccessfullLogin = 3;
    #endregion

    /// <summary>
    /// Calls the SendLoginWebRequestCoroutine function.
    /// </summary>
    public void SendServerLoginWebRequest()
    {
        StartCoroutine(SendServerLoginWebRequestCoroutine());
    }

    /// <summary>
    /// Send the actual data via a webrequest to login a existing user.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SendServerLoginWebRequestCoroutine()
    {
        string insertUserURL = "https://studentdav.hku.nl/~jaydee.alkema/databasing/login_server.php";

        WWWForm form = new WWWForm();
        form.AddField("servername", serverNameTextInputField.text);
        form.AddField("password", passwordTextInputfield.text);

        using (UnityWebRequest www = UnityWebRequest.Post(insertUserURL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string returnText = www.downloadHandler.text;
                if (returnText.ToLower().Contains("Wrong"))
                {
                    userFeedbackMessageText.color = Color.red;
                    userFeedbackMessageText.text = www.downloadHandler.text;
                }
                else
                {
                    GameManager.Instance.ServerLoggedIn = true;

                    userFeedbackMessageText.color = Color.green;
                    userFeedbackMessageText.text = "Login successful! Loading...";
                    yield return new WaitForSeconds(2);

                    PlayerPrefs.SetString("session_id", www.downloadHandler.text);
                    GameManager.Instance.ToggleUIPanel(uiPanelToToggleOnSuccessfullLogin);
                }
            }
        }
    }
}
