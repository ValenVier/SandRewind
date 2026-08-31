using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Minimal level-flow manager: restart on death, win entry point for LevelGoal</summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void RestartLevel()
    {
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void CompleteLevel()
    {
        Debug.Log("Level complete! Reached the goal with the coin.");
        Time.timeScale = 0f; // swap for a proper win screen later
    }
}