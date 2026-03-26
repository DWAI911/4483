using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Pause menu with resume, restart, and quit options.
/// Unity 2022.3.62f1 compatible.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private GameStateManager gameState;

    private void Awake()
    {
        gameState = GameStateManager.Instance;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (shopButton != null) shopButton.onClick.AddListener(OpenShop);
        if (restartButton != null) restartButton.onClick.AddListener(RestartRun);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
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

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
