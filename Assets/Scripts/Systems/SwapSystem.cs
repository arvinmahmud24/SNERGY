using UnityEngine;

public class SwapSystem : MonoBehaviour
{
    [SerializeField] private CharacterBase[] player1Characters = new CharacterBase[2];
    [SerializeField] private CharacterBase[] player2Characters = new CharacterBase[2];

    private int player1ActiveIndex = 0;
    private int player2ActiveIndex = 0;

    private void Start()
    {
        // Initialize: set first character as active
        SetPlayerActive(1, 0);
        SetPlayerActive(2, 0);
    }

    private void Update()
    {
        if (InputManager.Instance.GetSwapPressed())
        {
            // TODO: Determine which player based on input context
            // For now, assuming single-player swap test
            SwapPlayer(1);
        }
    }

    public void SwapPlayer(int playerNumber)
    {
        if (playerNumber == 1)
        {
            player1ActiveIndex = 1 - player1ActiveIndex; // Toggle 0 ? 1
            SetPlayerActive(1, player1ActiveIndex);
        }
        else if (playerNumber == 2)
        {
            player2ActiveIndex = 1 - player2ActiveIndex;
            SetPlayerActive(2, player2ActiveIndex);
        }
    }

    private void SetPlayerActive(int playerNumber, int charIndex)
    {
        CharacterBase[] characters = playerNumber == 1 ? player1Characters : player2Characters;

        // Deactivate all
        foreach (CharacterBase character in characters)
        {
            if (character != null)
                character.SetActive(false);
        }

        // Activate selected
        if (characters[charIndex] != null)
        {
            characters[charIndex].SetActive(true);
            Debug.Log($"Player {playerNumber} swapped to {characters[charIndex].gameObject.name}");
        }
    }

    public CharacterBase GetActiveCharacter(int playerNumber)
    {
        if (playerNumber == 1)
            return player1Characters[player1ActiveIndex];
        else
            return player2Characters[player2ActiveIndex];
    }

    public CharacterBase GetStandbyCharacter(int playerNumber)
    {
        if (playerNumber == 1)
            return player1Characters[1 - player1ActiveIndex];
        else
            return player2Characters[1 - player2ActiveIndex];
    }
}