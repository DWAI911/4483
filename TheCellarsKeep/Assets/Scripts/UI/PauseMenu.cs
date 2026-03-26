using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Pause menu with options for resume, shop, restart, and quit.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private GameStateManager gameState;

    private void Awake()
    {
        gameState = GameStateManager.Instance;
        
        // Initially hidden
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Set up button listeners
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (shopButton != null)
        {
            shopButton.onClick.AddListener(OpenShop);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartRun);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (gameState == null) return;

        if (gameState.IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (gameState != null)
        {
            gameState.TogglePause();
        }
        else
        {
            Time.timeScale = 0f;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (gameState != null && gameState.IsPaused)
        {
            gameState.TogglePause();
        }
        else
        {
            Time.timeScale = 1f;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenShop()
    {
        // This would open the shop UI or load shop scene
        if (gameState != null)
        {
            gameState.OpenShop();
        }
    }

    public void RestartRun()
    {
        Time.timeScale = 1f;
        
        if (gameState != null)
        {
            gameState.RestartRun();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
