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
    #endregion

    /// <summary>
    /// Calls the SendLoginWebRequestCoroutine function.
    /// </summary>
    public void SendLoginWebRequest()
    {
        StartCoroutine(SendLoginWebRequestCoroutine());
    }

    /// <summary>
    /// Loads scene by given index.
    /// </summary>
    /// <param name="index"> Index of the scene. </param>
    public void LoadSceneByIndex(int index)
    {
        SceneManager.LoadScene(index);
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

                    UserData user = JsonUtility.FromJson<UserData>(www.downloadHandler.text);
                    user.SaveDataToPlayerPrefs();

                    LoadSceneByIndex(2);
                }
            }
        }
    }
}

[System.Serializable]
public class UserData
{
    public string id;
    public string username;
    public string first_name;
    public string last_name;
    public string password;
    public string email;
    public string birth_date;
    public string register_date;
    public string last_login_date;

    /// <summary>
    /// Saves all userdata to playerprefs. Is this secure? No. Is this convenient? Yes!
    /// </summary>
    public void SaveDataToPlayerPrefs()
    {
        PlayerPrefs.SetInt("id", int.Parse(id));
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.SetString("first_name", first_name);
        PlayerPrefs.SetString("last_name", last_name);
        PlayerPrefs.SetString("password", password);
        PlayerPrefs.SetString("email", email);
        PlayerPrefs.SetString("birth_date", birth_date);
        PlayerPrefs.SetString("register_date", register_date);
        PlayerPrefs.SetString("last_login_date", last_login_date);
    }

    /// <summary>
    /// Get all UserData from PlayerPrefs.
    /// </summary>
    public void GetDataFromPlayerPrefs()
    {
        id = PlayerPrefs.GetInt("id").ToString();
        username = PlayerPrefs.GetString("username");
        first_name = PlayerPrefs.GetString("first_name");
        last_name = PlayerPrefs.GetString("last_name");
        password = PlayerPrefs.GetString("password");
        email = PlayerPrefs.GetString("email");
        birth_date = PlayerPrefs.GetString("birth_date");
        register_date = PlayerPrefs.GetString("register_date");
        last_login_date = PlayerPrefs.GetString("last_login_date");
    }
}
