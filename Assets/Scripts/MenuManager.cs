/*
* Author: Jason Koh
* Date: 16 August 2025
* Description: This script manages the game menu, including the start menu and win state.
* It handles state transitions, displays congratulatory messages, and allows the player to restart the game.
* The script uses coroutines to manage different game states and UI updates.
* It is designed to be attached to the player's GameObject.
* The script also includes a method to handle player interactions with the game world, such as reaching a destination.
* The congratulatory message is displayed when the player completes the level, showing the time taken and stars collected.
* The script uses Unity's UI system to display text and buttons for user interaction.
* It requires a RectTransform for the congratulatory background, a TMP_Text for the congratulatory message, and a Button for replay functionality.
*/

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

    /// <summary>
    /// Start is called before the first frame update.
    /// Initializes the menu manager, sets up the UI elements, and starts the game in the StartMenu state.
    /// It hides the congratulatory background and text initially.
    /// </summary>
    void Start()
    {
        congratulatoryBackground.gameObject.SetActive(false);
        congratulatoryText.gameObject.SetActive(false);
        replayButton.gameObject.SetActive(false);
        StartCoroutine(SwitchState("StartMenu"));
    }

    /// <summary>
    /// Switches the game state to the specified state.
    /// This method is used to transition between different game states such as StartMenu, Running, and Win.
    /// It starts the corresponding coroutine for the new state.
    /// </summary>
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

    /// <summary>
    /// Coroutine that handles the StartMenu state of the game.
    /// It freezes the game, displays the congratulatory background and text, and waits for the player to press Space to start the game.
    /// Once Space is pressed, it transitions to the Running state.
    /// </summary>
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

    /// <summary>
    /// Coroutine that handles the Running state of the game.
    /// It resumes the game, allowing player interactions and gameplay to continue.
    /// This method is called when the game starts or resumes after being paused.
    /// </summary>
    private IEnumerator GameRunning()
    {
        Debug.Log("Game Running...");

        Time.timeScale = 1f; // Resume the game
        yield break;
    }

    /// <summary>
    /// Coroutine that handles the Win state of the game.
    /// It displays a congratulatory message with the time taken and stars collected, and allows the player to restart the game.
    /// It calculates the final score based on the player's performance, including time taken and stars collected.
    /// The game is paused while this state is active, and the player can click the replay button to restart.
    /// </summary>
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

    /// <summary>
    /// This method is called when the player reaches the destination.
    /// It triggers the win state and displays the congratulatory message.
    /// The game is paused, and the player can choose to replay the game.
    /// </summary>
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
