using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class LoginUser : MonoBehaviour
{
    #region Privates
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField usernameTextInputfield = default;
    [SerializeField] private TMP_InputField passwordTextInputfield = default;
    [Space]
    [SerializeField] private TMP_Text userFeedbackMessageText = default;
    [SerializeField] private int uiPanelToToggleOnSuccessfullLogin = 4;
    #endregion

    /// <summary>
    /// Calls the SendLoginWebRequestCoroutine function.
    /// </summary>
    public void SendUserLoginWebRequest()
    {
        StartCoroutine(SendUserLoginWebRequestCoroutine());
    }

    /// <summary>
    /// Send the actual data via a webrequest to login a existing user.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SendUserLoginWebRequestCoroutine()
    {
        string insertUserURL = "https://studentdav.hku.nl/~jaydee.alkema/databasing/login_user.php";

        string session_id = PlayerPrefs.GetInt("session_id").ToString();

        if (session_id == "")
        {
            userFeedbackMessageText.color = Color.red;
            userFeedbackMessageText.text = "No session id found! Please login into a server first!";
            yield return null;
        }

        WWWForm form = new WWWForm();
        form.AddField("username", usernameTextInputfield.text);
        form.AddField("password", passwordTextInputfield.text);
        form.AddField("session_id", session_id);

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
                if (returnText.ToLower().Contains("wrong"))
                {
                    userFeedbackMessageText.color = Color.red;
                    userFeedbackMessageText.text = www.downloadHandler.text;
                }
                else
                {
                    userFeedbackMessageText.color = Color.green;
                    userFeedbackMessageText.text = "Login successful! Loading...";

                    UserData user = JsonUtility.FromJson<UserData>(www.downloadHandler.text);
                    user.SaveDataToPlayerPrefs();
                    yield return new WaitForSeconds(2);
                    GameManager.Instance.ToggleUIPanel(uiPanelToToggleOnSuccessfullLogin);
                }
            }
        }
    }
}