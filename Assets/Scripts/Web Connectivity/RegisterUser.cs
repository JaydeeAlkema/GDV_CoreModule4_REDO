using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class RegisterUser : MonoBehaviour
{
    #region Privates
    [Header("Settings")]
    [SerializeField] private int minUsernameCharacterAmount = 6;
    [SerializeField] private int minPasswordCharacterAmount = 8;
    [SerializeField] private bool loadUserDataOnSceneLoad = false;
    [SerializeField] private int uiPanelToToggleOnSuccessfullRegister = 3;

    [Header("Misc References")]
    [SerializeField] private Transform panelTransform = default;

    [Header("UI Elements")]
    [Space]
    [SerializeField] private TMP_InputField usernameTextInputfield = default;
    [Space]
    [SerializeField] private TMP_InputField firstNameTextInputfield = default;
    [SerializeField] private TMP_InputField lastNameTextInputfield = default;
    [SerializeField] private TMP_InputField emailTextInputfield = default;
    [Space]
    [SerializeField] private TMP_InputField passwordTextInputfield = default;
    [SerializeField] private TMP_InputField passwordConfirmTextInputfield = default;
    [Space]
    [SerializeField] private TMP_InputField birthdateDayTextInputfield = default;
    [SerializeField] private TMP_InputField birthdateMonthTextInputfield = default;
    [SerializeField] private TMP_InputField birthdateYearTextInputfield = default;
    [Space]
    [SerializeField] private TMP_Text userFeedbackMessageText = default;

    private bool validInput = false; // this HAS to be true for the register form the be send successfully.
    private int customErrorCode = 0; // 0 is good, everything else is bad.
    #endregion

    private void OnEnable()
    {
        if (loadUserDataOnSceneLoad)
        {
            UserData userData = new UserData();
            userData.GetDataFromPlayerPrefs();

            if (userData != null && userData.id != "0")
            {
                usernameTextInputfield.text = userData.username;
                firstNameTextInputfield.text = userData.first_name;
                lastNameTextInputfield.text = userData.last_name;
                emailTextInputfield.text = userData.email;
                passwordTextInputfield.text = userData.password;
                passwordConfirmTextInputfield.text = userData.password;

                DateTime dateTime = DateTime.Parse(userData.birth_date);
                birthdateDayTextInputfield.text = dateTime.Day.ToString();
                birthdateMonthTextInputfield.text = dateTime.Month.ToString();
                birthdateYearTextInputfield.text = dateTime.Year.ToString();
            }
        }
    }

    /// <summary>
    /// Validates the input by checking many use cases. This is purely for the input form, this exact process is repeated in the PHP code.
    /// </summary>
    public void ValidateInputs()
    {
        // Check if any of the fields is left empty.
        TMP_InputField[] inputfields = panelTransform.GetComponentsInChildren<TMP_InputField>();
        foreach (var inputfield in inputfields)
        {
            if (inputfield.text == string.Empty)
            {
                userFeedbackMessageText.color = Color.red;
                userFeedbackMessageText.text = "All fields are required to be filled in!";
                return;
            }
        }

        if (usernameTextInputfield.text.Length < minUsernameCharacterAmount)
        {
            userFeedbackMessageText.color = Color.red;
            userFeedbackMessageText.text = $"Username is too short! (Minimum of {minUsernameCharacterAmount} characters)";
        }
        else if (passwordTextInputfield.text.Length < minPasswordCharacterAmount || passwordConfirmTextInputfield.text.Length < minPasswordCharacterAmount)
        {
            userFeedbackMessageText.color = Color.red;
            userFeedbackMessageText.text = $"Password is too short! (Minimum of {minPasswordCharacterAmount} characters)";
        }
        else if (passwordTextInputfield.text != passwordConfirmTextInputfield.text)
        {
            userFeedbackMessageText.color = Color.red;
            userFeedbackMessageText.text = "Passwords do not match!";
        }
        else if (emailTextInputfield.text.Contains("@") == false) // This is only partial check, in partice someone could add an '@' but leave out '.com' or '.info' making the emial invalid still.
        {
            userFeedbackMessageText.color = Color.red;
            userFeedbackMessageText.text = "Email must contain '@'!";
        }
        else
        {
            validInput = true;
        }
    }

    /// <summary>
    /// Calls the SendRegisterWebRequestCoroutine function.
    /// </summary>
    public void SendRegisterWebRequest()
    {
        ValidateInputs();
        if (!validInput) return;

        StartCoroutine(SendRegisterWebRequestCoroutine());
    }

    /// <summary>
    /// Calls the SendSaveChangesWebRequestCoroutine function.
    /// </summary>
    public void SendSaveChangesWebRequest()
    {
        ValidateInputs();
        if (!validInput) return;

        StartCoroutine(SendSaveChangesWebRequestCoroutine());
    }

    /// <summary>
    /// Send the actual data via a webrequest to insert a new user into the database.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SendRegisterWebRequestCoroutine()
    {
        string insertUserURL = "https://studentdav.hku.nl/~jaydee.alkema/databasing/insert_user.php";
        long birthdateToTimeStamp = GetTimestampFromDateTime(int.Parse(birthdateDayTextInputfield.text), int.Parse(birthdateMonthTextInputfield.text), int.Parse(birthdateYearTextInputfield.text));

        WWWForm form = new WWWForm();
        form.AddField("username", usernameTextInputfield.text);
        form.AddField("first_name", firstNameTextInputfield.text);
        form.AddField("last_name", lastNameTextInputfield.text);
        form.AddField("password", passwordTextInputfield.text);
        form.AddField("email", emailTextInputfield.text);
        form.AddField("birth_date", birthdateToTimeStamp.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post(insertUserURL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string errorMessage = www.downloadHandler.text;
                if (errorMessage.ToLower().Contains("error"))
                {
                    string[] splitErrorMessage = errorMessage.Split(':');
                    customErrorCode = int.Parse(splitErrorMessage[1]);
                    HandleCustomErrorCode();
                }
                else
                {
                    customErrorCode = 0;
                }
            }

            if (customErrorCode == 0)
            {
                userFeedbackMessageText.color = Color.green;
                userFeedbackMessageText.text = "Account created successfully! Forwarding to login page...";
                yield return new WaitForSeconds(2);

                GameManager.Instance.ToggleUIPanel(uiPanelToToggleOnSuccessfullRegister);
            }
            else
            {
                yield return null;
            }
        }
    }

    /// <summary>
    /// Send the actual data via a webrequest to insert a new user into the database.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SendSaveChangesWebRequestCoroutine()
    {
        string insertUserURL = "https://studentdav.hku.nl/~jaydee.alkema/databasing/insert_user.php";
        long birthdateToTimeStamp = GetTimestampFromDateTime(int.Parse(birthdateDayTextInputfield.text), int.Parse(birthdateMonthTextInputfield.text), int.Parse(birthdateYearTextInputfield.text));

        UserData userData = new UserData();
        userData.GetDataFromPlayerPrefs();

        if (userData.id == "0")
        {
            yield return null;
        }

        WWWForm form = new WWWForm();
        form.AddField("id", userData.id);
        form.AddField("username", usernameTextInputfield.text);
        form.AddField("first_name", firstNameTextInputfield.text);
        form.AddField("last_name", lastNameTextInputfield.text);
        form.AddField("password", passwordTextInputfield.text);
        form.AddField("email", emailTextInputfield.text);
        form.AddField("birth_date", birthdateToTimeStamp.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post(insertUserURL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string errorMessage = www.downloadHandler.text;
                if (errorMessage.ToLower().Contains("error"))
                {
                    Debug.Log(errorMessage);
                    string[] splitErrorMessage = errorMessage.Split(':');
                    customErrorCode = int.Parse(splitErrorMessage[1]);
                    HandleCustomErrorCode();
                }
                else
                {
                    customErrorCode = 0;
                }
            }

            if (customErrorCode == 0)
            {
                userFeedbackMessageText.color = Color.green;
                userFeedbackMessageText.text = "Saved changes successfully!";

                userData.username = usernameTextInputfield.text;
                userData.first_name = firstNameTextInputfield.text;
                userData.last_name = lastNameTextInputfield.text;
                userData.password = passwordTextInputfield.text;
                userData.email = emailTextInputfield.text;
                //userData.birth_date = new DateTime(int.Parse(birthdateDayTextInputfield.text), int.Parse(birthdateMonthTextInputfield.text), int.Parse(birthdateYearTextInputfield.text)).ToString();
                userData.SaveDataToPlayerPrefs();

                Debug.Log(new DateTime(int.Parse(birthdateDayTextInputfield.text), int.Parse(birthdateMonthTextInputfield.text), int.Parse(birthdateYearTextInputfield.text)).ToString());
            }
            else
            {
                yield return null;
            }
        }
    }

    /// <summary>
    /// Convert Day, Month & Year Datetime to Timestamp
    /// </summary>
    /// <returns> Datetime to Timestamp (as long) </returns>
    private long GetTimestampFromDateTime(int day, int month, int year)
    {
        DateTime dateTime = new DateTime(year, month, day);
        return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Gives the user an error message based on the custom error code. (Which is returned from the webrequest)
    /// </summary>
    private void HandleCustomErrorCode()
    {
        switch (customErrorCode)
        {
            case 1:
                userFeedbackMessageText.color = Color.red;
                userFeedbackMessageText.text = "Email already in use!";
                break;

            case 2:
                userFeedbackMessageText.color = Color.red;
                userFeedbackMessageText.text = "Username already in use!";
                break;

            default:
                break;
        }
    }
}
