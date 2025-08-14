using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int totalLevels; // No longer needs a default value here
    public int currentLevel = 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // --- THIS IS THE FIX ---
            // Load the total number of levels that was saved by the Main Menu.
            // If no value was saved, default to 5 levels.
            totalLevels = PlayerPrefs.GetInt("TotalLevels", 5);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GoToNextLevel()
    {
        if (currentLevel < totalLevels)
        {
            currentLevel++;
            Debug.Log("Proceeding to Level " + currentLevel);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            WinGame();
        }
    }

    private void WinGame()
    {
        Debug.Log("You have cleared all the levels! YOU WIN!");
        // IMPORTANT: Make sure your main menu scene is named "MainMenu" in your Build Settings.
        SceneManager.LoadScene("MainMenu");
    }
}
