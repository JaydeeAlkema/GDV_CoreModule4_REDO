using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class HighscorePanel : MonoBehaviour
{
    #region Privates
    [Space]
    [BoxGroup("UI Variables"), SerializeField] private GameObject textPrefab = default;
    [BoxGroup("UI Variables"), SerializeField] private Transform usersTextParentTransform = default;
    [BoxGroup("UI Variables"), SerializeField] private Transform scoresTextParentTransform = default;
    [BoxGroup("UI Variables"), SerializeField] private TMP_Text totalPlaysInLastMonthText = default;

    [Space]
    [SerializeField] private List<HighscoresData> highscores = new List<HighscoresData>();
    // the offset is how many month in the past we fetch highscores. so 1 offset is all score from today till exactly 1 month in the past.
    [SerializeField] private int monthOffset1 = 1;
    [SerializeField] private int monthOffset2 = 0;
    [SerializeField] private int totalPlays = 0;
    #endregion

    /// <summary>
    /// Calls the SendGetHighscoresWebRequestCoroutine function.
    /// </summary>
    private void OnEnable()
    {
        StartCoroutine(SendGetHighscoresFromCurrentMonthWebRequestCoroutine());
    }

    // Set offset button methods. Not very neat, I know, but it works. Don't judge :)
    public void SetMonthOffset1(int offset)
    {
        monthOffset1 = offset;
    }
    public void SetMonthOffset2(int offset)
    {
        monthOffset2 = offset;
    }
    public void SendGetHighscoresFromCurrentMonthWebRequest()
    {
        StartCoroutine(SendGetHighscoresFromCurrentMonthWebRequestCoroutine());
    }

    /// <summary>
    /// Send the actual data via a webrequest to get highscore data.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SendGetHighscoresFromCurrentMonthWebRequestCoroutine()
    {
        string insertUserURL = "https://studentdav.hku.nl/~jaydee.alkema/databasing/get_leaderboard_statistics_by_month.php";

        string sessionID = PlayerPrefs.GetString("session_id");

        WWWForm form = new WWWForm();
        form.AddField("session_id", sessionID);
        form.AddField("interval1", monthOffset1);
        form.AddField("interval2", monthOffset2);

        // Clean up all the UI elements each time the scores get called.
        highscores.Clear();
        Transform[] userTextElements = usersTextParentTransform.GetComponentsInChildren<Transform>();
        foreach (Transform transform in userTextElements)
        {
            if (transform != usersTextParentTransform)
            {
                Destroy(transform.gameObject);
            }
        }
        Transform[] scoreTextElements = scoresTextParentTransform.GetComponentsInChildren<Transform>();
        foreach (Transform transform in scoreTextElements)
        {
            if (transform != scoresTextParentTransform)
            {
                Destroy(transform.gameObject);
            }
        }

        yield return new WaitForEndOfFrame();
        using (UnityWebRequest www = UnityWebRequest.Post(insertUserURL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string[] splitData = www.downloadHandler.text.Split(new string[] { "{", "}", "[", "]", ",", ":", "\"", "username", "score", "plays" }, System.StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < splitData.Length - 2; i += 2)
                {
                    HighscoresData highscoresData = new HighscoresData();
                    highscoresData.username = splitData[i];
                    highscoresData.score = splitData[i + 1];
                    highscores.Add(highscoresData);

                    GameObject userTextGO = Instantiate(textPrefab, usersTextParentTransform);
                    userTextGO.GetComponent<TextMeshProUGUI>().text = highscoresData.username;

                    GameObject scoreTextGO = Instantiate(textPrefab, scoresTextParentTransform);
                    scoreTextGO.GetComponent<TextMeshProUGUI>().text = highscoresData.score;
                }

                totalPlays = int.Parse(splitData[splitData.Length - 1]);
                totalPlaysInLastMonthText.text = $"total Plays: {totalPlays}";
            }
        }
    }
}

[System.Serializable]
public class HighscoresData
{
    public string username;
    public string score;
}
