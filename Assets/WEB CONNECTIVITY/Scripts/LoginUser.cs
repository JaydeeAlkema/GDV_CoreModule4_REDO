using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoginUser : MonoBehaviour
{
    #region Privates
    [Header("UI Elements")]
    [SerializeField] private TMP_InputField usernameTextInputfield = default;
    [SerializeField] private TMP_InputField passwordTextInputfield = default;
    [Space]
    [SerializeField] private TMP_Text userFeedbackMessageText = default;

    private int customErrorCode = 0; // 0 is good, everything else is bad.
    #endregion

    /// <summary>
    /// Calls the SendLoginWebRequestCoroutine function.
    /// </summary>
    public void SendLoginWebRequest()
    {
        StartCoroutine(SendLoginWebRequestCoroutine());
    }

    /// <summary>
    /// Send the actual data via a webrequest to login a existing user.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SendLoginWebRequestCoroutine()
    {
        string insertUserURL = "https://studentdav.hku.nl/~jaydee.alkema/databasing/login_user.php";

        WWWForm form = new WWWForm();
        form.AddField("username", usernameTextInputfield.text);
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
                if (returnText.ToLower().Contains("wrong"))
                {
                    userFeedbackMessageText.color = Color.red;
                    userFeedbackMessageText.text = www.downloadHandler.text;
                }
                else
                {
                    userFeedbackMessageText.color = Color.green;
                    userFeedbackMessageText.text = "Login successful! Loading...";
                }
            }
        }
    }
}
