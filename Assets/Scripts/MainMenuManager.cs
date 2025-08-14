using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Required for TextMeshPro UI elements

public class MainMenuManager : MonoBehaviour
{
    public TMP_InputField levelInput;

    public void StartGame()
    {
        // Get the text from the input field
        string levelText = levelInput.text;

        // Convert the text to a number
        if (int.TryParse(levelText, out int totalLevels) && totalLevels > 0)
        {
            // Store the number of levels so the GameManager can find it
            PlayerPrefs.SetInt("TotalLevels", totalLevels);

            // Load the main game scene
            // IMPORTANT: Make sure your game scene is named "GameScene"
            SceneManager.LoadScene("Game");
        }
        else
        {
            Debug.Log("Please enter a valid number of levels.");
        }
    }
}