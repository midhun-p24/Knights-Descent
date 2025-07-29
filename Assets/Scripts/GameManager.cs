using UnityEngine;
using UnityEngine.SceneManagement; // for reload or scene-load

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void WinGame()
    {
        Debug.Log("You Win!");
        // TODO: show UI, then restart or load next level
    }
}
