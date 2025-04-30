using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WebLoginState : MonoBehaviour
{
    public UnityEngine.UI.Text loginName;
    public GameObject loginForm;

    public void Login()
    {
        loginName.text = Steamworks.SteamUser.GetSteamID().ToString();
        // StartCoroutine(TryLogin());
        string sessionKey = LoadSessionKey();
        if (string.IsNullOrEmpty(sessionKey))
        {
            StartCoroutine(LoginWithSteamTicket(loginName.text));
        }
        else
        {
            StartCoroutine(LoginWithSessionKey(loginName.text, sessionKey));
        }
    }

    private void Start()
    {
        Debug.Log("SteamAPI:" + Steamworks.SteamAPI.Init());
    }

    private void Update()
    {
        //Steamworks.SteamAPI.up
        if (Steamworks.SteamAPI.IsSteamRunning())
        {
            Steamworks.SteamAPI.RunCallbacks();
            loginName.text = Steamworks.SteamUser.GetSteamID().ToString();
        }
    }

    string LoadSessionKey()
    {
        return PlayerPrefs.GetString("oatom_session_key", "");
    }

    void SaveSessionKey(string newKey)
    {
        PlayerPrefs.SetString("oatom_session_key", newKey);
        PlayerPrefs.Save();
    }

    [System.Serializable]
    private class SessionResponse
    {
        public string status;
        public string steamid;
        public string session_key;
    }

    string ExtractSessionKeyFromJson(string json)
    {
        try
        {
            var parsed = JsonUtility.FromJson<SessionResponse>(json);
            return parsed.session_key;
        }
        catch
        {
            Debug.LogWarning("Failed to parse session key from JSON.");
            return null;
        }
    }

    IEnumerator LoginWithSessionKey(string steamID, string sessionKey)
    {
        WWWForm form = new WWWForm();
        form.AddField("steamid", steamID);
        form.AddField("session_key", sessionKey);

        UnityWebRequest www = UnityWebRequest.Post(ModEntryPoint.server + "login.php", form);
        yield return www.SendWebRequest();

        if (!www.isNetworkError && !www.isHttpError)
        {
            var response = www.downloadHandler.text;

            Debug.Log(response);
            {
                Debug.Log("Logged in with session key.");
                yield return OnLogged(steamID, sessionKey);
                yield break;
            }
        }

        Debug.LogWarning("Session key invalid. Logging in with Steam...");
        StartCoroutine(LoginWithSteamTicket(steamID));
    }

    IEnumerator LoginWithSteamTicket(string steamID)
    {
        byte[] ticketData = new byte[1024];
        uint ticketSize;
        Steamworks.HAuthTicket authTicket = Steamworks.SteamUser.GetAuthSessionTicket(ticketData, ticketData.Length, out ticketSize);

        byte[] trimmedTicket = new byte[ticketSize];
        System.Array.Copy(ticketData, trimmedTicket, ticketSize);
        string ticketHex = System.BitConverter.ToString(trimmedTicket).Replace("-", "");

        WWWForm form = new WWWForm();
        form.AddField("steamid", steamID);
        form.AddField("ticket", ticketHex);

        Debug.Log(steamID);
        Debug.Log(ticketHex);

        UnityWebRequest www = UnityWebRequest.Post(ModEntryPoint.server + "login.php", form);
        yield return www.SendWebRequest();

        string jsonResponse = www.downloadHandler.text;

        if (!www.isNetworkError && !www.isHttpError)
        {
            string newSessionKey = ExtractSessionKeyFromJson(jsonResponse);
            if (!string.IsNullOrEmpty(newSessionKey))
            {
                SaveSessionKey(newSessionKey);
                Debug.Log("Logged in with Steam. Session key saved.");
                yield return OnLogged(steamID, newSessionKey);
                yield break;
            }
        }

        Debug.LogError("Steam login failed: " + www.error + " = " + jsonResponse);
    }

    IEnumerator OnLogged(string userId, string sessionKey)
    {
        WebRequest request = new WebRequest();
        yield return request.Do(ModEntryPoint.server + "get_characters.php",
            new MultipartFormDataSection("user_id", userId),
            new MultipartFormDataSection("session_key", sessionKey)
            );

        if (request.Success)
        {
            var characters = request.GetData()["characters"].AsArray;
            if (characters.Count == 0)
            {
                yield return CreateCharacter(userId, sessionKey, Steamworks.SteamFriends.GetPersonaName(), characters);
            }

            if (characters.Count == 0)
            {
                Debug.LogError("Can't create character");
                yield break;
            }

            var character = characters[0];

            GlobalEvents.PerformEvent<OnlineEvents.Login>(new OnlineEvents.Login
            {
                sessionKey = sessionKey,
                uid = character["id"].AsInt,
                world_tile = character["world_tile"].AsInt,
                cell = character["cell"].AsInt,
            });

            loginForm.SetActive(false);
        }
    }

    IEnumerator CreateCharacter(string userId, string sessionKey, string nickname, JSon.JArray characters)
    {
        WebRequest request = new WebRequest();
        yield return request.Do(ModEntryPoint.server + "create_character.php",
            new MultipartFormDataSection("user_id", userId),
            new MultipartFormDataSection("session_key", sessionKey),
            new MultipartFormDataSection("char_name", nickname)
            );

        if (request.Success)
        {
            characters.Add(request.GetData()["character"]);
        }
    }
}
