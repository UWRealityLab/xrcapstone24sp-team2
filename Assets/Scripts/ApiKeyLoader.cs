using System; // For Exception
using UnityEngine;
using System.IO;

[System.Serializable]
public class Auth
{
    public string api_key;       // Match the JSON property name "api_key"
    public string organization;  // Match the JSON property name "organization"
}

public static class ApiKeyLoader
{
    public static string LoadApiKey()
    {
        string userPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        string authPath = Path.Combine(userPath, ".openai", "auth.json");
        if (File.Exists(authPath))
        {
            try
            {
                var json = File.ReadAllText(authPath);
                var auth = JsonUtility.FromJson<Auth>(json);
                if (auth != null && !string.IsNullOrEmpty(auth.api_key))
                {
                    return auth.api_key;
                }
                else
                {
                    Debug.LogError("Failed to parse the API key from auth.json.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error reading or parsing auth.json: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("auth.json file not found. Please ensure it exists at: " + authPath);
        }
        return null;
    }
}
