using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject completeLevelUI;
    public GameObject gameOverUI;

    public float restartDelay = 1f;

    public void EndGame()
    {
        gameOverUI.SetActive(true);
    }

    public void CompleteLevel()
    {
        completeLevelUI.SetActive(true);
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
