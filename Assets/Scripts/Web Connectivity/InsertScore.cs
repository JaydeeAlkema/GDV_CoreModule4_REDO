using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class InsertScore : MonoBehaviour
{
    [SerializeField] private TMP_InputField userIdInputfield = default;
    [SerializeField] private TMP_InputField scoreInputField = default;
    [Space]
    [SerializeField] private TextMeshProUGUI sessionIdText = default;
    [SerializeField] private TextMeshProUGUI userFeedbackMessageText = default;

    public void SendInsertScoreWebRequest()
    {
        StartCoroutine(SendInsertScoreWebRequestCoroutine());
    }

    private IEnumerator SendInsertScoreWebRequestCoroutine()
    {
        string insertUserURL = "https://studentdav.hku.nl/~jaydee.alkema/databasing/insert_score.php";

        string sessionID = PlayerPrefs.GetString("session_id");
        Debug.Log(PlayerPrefs.GetString("session_id"));
        sessionIdText.text = sessionID;

        WWWForm form = new WWWForm();
        form.AddField("session_id", sessionID);
        form.AddField("score", scoreInputField.text);
        form.AddField("user_id", userIdInputfield.text);

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
                if (returnText.ToLower().Contains("error"))
                {
                    userFeedbackMessageText.color = Color.red;
                    userFeedbackMessageText.text = returnText;
                }
                else
                {
                    GameManager.Instance.ServerLoggedIn = true;

                    userFeedbackMessageText.color = Color.green;
                    userFeedbackMessageText.text = returnText;
                }
            }
        }
    }
}
