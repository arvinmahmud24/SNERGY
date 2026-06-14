using UnityEngine;

public class Test_GeminiAI : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestAI();
        }
    }

    private void TestAI()
    {
        string prompt = "Apa itu game SYNERGY? Jelaskan dalam 1 kalimat.";

        Debug.Log("🤖 Asking Gemini...");

        GeminiAI.Instance.SendPrompt(prompt, (response) =>
        {
            Debug.Log("✅ Response: " + response);
        });
    }
}