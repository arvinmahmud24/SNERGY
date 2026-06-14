using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GeminiAI : MonoBehaviour
{
    private const string API_KEY = "AQ.Ab8RN6IFlxi7Z8CpuaZZ5fuFmz5Evg_8bkCj0swgH5d7_lNCqw"; // ← Replace!
    // UPDATED: gemini-pro → gemini-1.5-flash
    private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

    public static GeminiAI Instance { get; private set; }

    public delegate void OnResponseReceived(string response);

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void SendPrompt(string prompt, OnResponseReceived callback)
    {
        StartCoroutine(SendPromptCoroutine(prompt, callback));
    }

    private IEnumerator SendPromptCoroutine(string prompt, OnResponseReceived callback)
    {
        string jsonRequest = BuildJsonRequest(prompt);

        Debug.Log("📤 Sending to Gemini: " + prompt);

        using (UnityWebRequest request = new UnityWebRequest(API_URL + "?key=" + API_KEY, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonRequest);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("📥 Response: " + responseText);

                GeminiResponse response = JsonUtility.FromJson<GeminiResponse>(responseText);

                if (response != null && response.candidates != null && response.candidates.Length > 0)
                {
                    string aiResponse = response.candidates[0].content.parts[0].text;
                    callback?.Invoke(aiResponse);
                }
                else
                {
                    callback?.Invoke("No response from Gemini");
                }
            }
            else
            {
                Debug.LogError("❌ Error: " + request.error);
                callback?.Invoke("Error: " + request.error);
            }
        }
    }

    private string BuildJsonRequest(string prompt)
    {
        string escapedPrompt = prompt.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");

        string json = "{\"contents\":[{\"parts\":[{\"text\":\"" + escapedPrompt + "\"}]}]}";

        return json;
    }

    [System.Serializable]
    public class GeminiResponse
    {
        public Candidate[] candidates;
    }

    [System.Serializable]
    public class Candidate
    {
        public Content content;
    }

    [System.Serializable]
    public class Content
    {
        public Part[] parts;
    }

    [System.Serializable]
    public class Part
    {
        public string text;
    }
}