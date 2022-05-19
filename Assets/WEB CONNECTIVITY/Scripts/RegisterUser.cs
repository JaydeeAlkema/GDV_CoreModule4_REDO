using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RegisterUser : MonoBehaviour
{
    #region Privates
    [Header("Settings")]
    [SerializeField] private int minUsernameCharacterAmount = 6;
    [SerializeField] private int minPasswordCharacterAmount = 8;

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
    #endregion

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
        else if (emailTextInputfield.text.Contains("@") == false)
        {
            userFeedbackMessageText.color = Color.red;
            userFeedbackMessageText.text = "Email must contain '@'!";
        }
        else
        {
            userFeedbackMessageText.color = Color.green;
            userFeedbackMessageText.text = "Successfully created account! Redirecting to Login page...";
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
    /// Send the actual data via a webrequest to insert a new user into the database.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SendRegisterWebRequestCoroutine()
    {
        yield return new WaitForEndOfFrame();

        StartCoroutine(LoadLoginPage());
    }

    private IEnumerator LoadLoginPage()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Login");
    }
}
