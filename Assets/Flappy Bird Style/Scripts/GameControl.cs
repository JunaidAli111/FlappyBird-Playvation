using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameControl : MonoBehaviour 
{

	public delegate void GameDelegate(); //A delegate to reference the events on game start and game restart events.
	public static event GameDelegate OnGameStarted; //Event that triggers when the game is started (Registered in Bird Script).
	public static event GameDelegate OnGameOverConfirmed; //Even that triggers when the user hit replay button after the game is over.

	public static GameControl instance;			//A reference to our game control script so we can access it statically.
	public Text scoreText;						//A reference to the UI text component that displays the player's score.
	public GameObject gameOvertext;				//A reference to the object that displays the text which appears when the player dies.

	private int score = 0;						//The player's score.
	public bool gameOver = false;				//Is the game over?
	public float scrollSpeed = -1.5f;			//Speed with which the obstacles and background move


	public GameObject startMenuUI;				//Reference to the Menu UI (When the game is started)
	public GameObject gameOverUI;               //Reference to the Game Over UI (When the bird crashes on obstacles or ground)
	public GameObject countdownUI;              //Reference to the Countdown UI (When the user taps play button, game countdown starts)


	enum UserInterfaceState //Enum to reference the Userinterface states, depending on the state of the game
	{
		None,
		Start,
		Countdown,
		GameOver
	}

	void Awake()
	{
		//If we don't currently have a game control...
		if (instance == null)
			//...set this one to be it...
			instance = this;
		//...otherwise...
		else if(instance != this)
			//...destroy this one because it is a duplicate.
			Destroy (gameObject);
	}

	void Update()
	{
		//If the game is over and the player has pressed some input...
		//if (gameOver && Input.GetMouseButtonDown(0))
		//{
			//...reload the current scene.
			///SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		//}
	}

	/// <summary>
    /// Function to be registered as an event when the bird scores or passes through one of the obstacles.
    /// The event is registered in Bird script.
    /// </summary>
	public void BirdScored()
	{
		//The bird can't score if the game is over.
		if (gameOver)	
			return;
		//If the game is not over, increase the score...
		score++;
		//...and adjust the score text.
		scoreText.text = "Score: " + score.ToString();
	}


	/// <summary>
	/// Function to be registered as an event when the bird dies, crashing into obstacles or ground.
	/// The event is registered in Bird script.
	/// </summary>
	public void BirdDied()
	{
		gameOver = true;
		int savedScore = PlayerPrefs.GetInt("HighScore");
		if (score > savedScore)
		{
			PlayerPrefs.SetInt("HighScore", score);
		}

		SetPageState(UserInterfaceState.GameOver);
	}





	/// <summary>
	/// Subscribe to events when the game object becomes active.
	/// </summary>
	void OnEnable()
	{
		Bird.OnBirdDied += BirdDied;
		Bird.OnBirdScored += BirdScored;
		CountDownText.OnCountdownFinished += OnCountdownFinished;
	}

	/// <summary>
	/// Unsubscribe to events when the game object becomes inactive.
	/// </summary>
	void OnDisable()
	{
		Bird.OnBirdDied -= BirdDied;
		Bird.OnBirdScored -= BirdScored;
		CountDownText.OnCountdownFinished -= OnCountdownFinished;
	}

	/// <summary>
	/// Function to be registered as an event when the countdown Finishes
	/// </summary>
	void OnCountdownFinished()
	{
		//Disable all UIs because the game is supposed to start now
		SetPageState(UserInterfaceState.None);
		//Call the Game starting event to enable the physics operations on the bird and input from the user
		OnGameStarted();
		//Set the initial score
		score = 0;
		//Game is in running mode indicator
		gameOver = false;
		
	}


	/// <summary>
	/// Method to activate and deactivate the required UIs based on the current state of game
	/// </summary>
	void SetPageState(UserInterfaceState state)
	{
		switch (state)
		{
			case UserInterfaceState.None:
				startMenuUI.SetActive(false);
				gameOverUI.SetActive(false);
				countdownUI.SetActive(false);
				break;
			case UserInterfaceState.Start:
				startMenuUI.SetActive(true);
				gameOverUI.SetActive(false);
				countdownUI.SetActive(false);
				break;
			case UserInterfaceState.Countdown:
				startMenuUI.SetActive(false);
				gameOverUI.SetActive(false);
				countdownUI.SetActive(true);
				break;
			case UserInterfaceState.GameOver:
				startMenuUI.SetActive(false);
				gameOverUI.SetActive(true);
				countdownUI.SetActive(false);
				break;
		}
	}

	/// <summary>
	/// Function registered as button tap listener for the Replay button in the Game Over UI, displayed when the game is over.
	/// It calls OnGameOverConfirmed() event to reset all the fields and values and brings the game to its initial state.
	/// </summary>
	public void ConfirmGameOver()
	{
		SetPageState(UserInterfaceState.Start);
		scoreText.text = "Score: 0";
		OnGameOverConfirmed();
	}

	/// <summary>
	/// Function registered as button tap listener for the Play button in the Menu UI.
	/// It activates the Countdown UI to which eventually leads to starting of the gameplay.
	/// </summary>
	public void StartGame()
	{
		SetPageState(UserInterfaceState.Countdown);
	}
}
