using UnityEngine;

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
