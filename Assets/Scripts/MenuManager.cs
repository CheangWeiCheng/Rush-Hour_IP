using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [SerializeField] RectTransform congratulatoryBackground;
    [SerializeField] TMP_Text congratulatoryText;
    [SerializeField] private Button replayButton;

    private enum GameState { StartMenu, Running, Win }
    private GameState currentState;

    void Start()
    {
        congratulatoryBackground.gameObject.SetActive(false);
        congratulatoryText.gameObject.SetActive(false);
        replayButton.gameObject.SetActive(false);
        StartCoroutine(SwitchState("StartMenu"));
    }

    private IEnumerator SwitchState(string stateName)
    {
        // End current state
        if (currentState == GameState.StartMenu) StopCoroutine(nameof(GameStart));
        if (currentState == GameState.Running) StopCoroutine(nameof(GameRunning));
        if (currentState == GameState.Win) StopCoroutine(nameof(GameWin));

        // Switch to new state
        switch (stateName)
        {
            case "StartMenu":
                currentState = GameState.StartMenu;
                yield return StartCoroutine(GameStart());
                break;

            case "Running":
                currentState = GameState.Running;
                yield return StartCoroutine(GameRunning());
                break;

            case "Win":
                currentState = GameState.Win;
                yield return StartCoroutine(GameWin());
                break;
        }
    }

    private IEnumerator GameStart()
    {
        Time.timeScale = 0f; // Freeze the game
        congratulatoryBackground.gameObject.SetActive(true);
        congratulatoryText.text = "Press Space to Start";
        congratulatoryText.gameObject.SetActive(true);

        // Wait until the player presses Space (new Input System)
        while (!Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            yield return null;
        }

        Debug.Log("Starting game...");
        congratulatoryBackground.gameObject.SetActive(false);
        congratulatoryText.gameObject.SetActive(false);
        yield return StartCoroutine(SwitchState("Running"));
    }

    private IEnumerator GameRunning()
    {
        Debug.Log("Game Running...");

        Time.timeScale = 1f; // Resume the game
        yield break;
    }

    private IEnumerator GameWin()
    {
        Debug.Log("You have completed the game!");
        Time.timeScale = 0f; // Freeze the game
        congratulatoryBackground.gameObject.SetActive(true);
        // Get the elapsed time and score from the PlayerBehaviour script
        PlayerBehaviour player = FindFirstObjectByType<PlayerBehaviour>();
        if (player == null)
        {
            Debug.LogError("PlayerBehaviour not found in the scene.");
            yield break;
        }
        float elapsedTime = player.elapsedTime;
        int currentScore = player.currentScore;
        // Format the congratulatory message
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);

        congratulatoryText.text =
            $"<b>LEVEL COMPLETE!</b>\n" +
            $"Time Taken: {minutes:00}:{seconds:00}\n" +
            $"Stars Collected: {currentScore}\n\n" +
            $"<b>Final Score: {Mathf.Max(1000, (currentScore * 1000) + Mathf.Max(0, 10000 - (elapsedTime * 10))):N0}</b>";

        congratulatoryText.gameObject.SetActive(true);
        replayButton.gameObject.SetActive(true);
        replayButton.onClick.AddListener(() =>
        {
            // Reload the current scene to restart the game
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        });
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Destination"))
        {
            // Trigger the win state when reaching the destination
            Debug.Log("You have reached the destination!");
            StartCoroutine(SwitchState("Win"));
        }
    }
}
