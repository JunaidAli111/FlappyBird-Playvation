using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Linq;

public class GameControl : MonoBehaviour 
{

	public delegate void GameDelegate(); //A delegate to reference the events on game start and game restart events.
	public static event GameDelegate OnGameStarted; //Event that triggers when the game is started (Registered in Bird Script).
	public static event GameDelegate OnGameOverConfirmed; //Even that triggers when the user hit replay button after the game is over.

	public static GameControl instance;			//A reference to our game control script so we can access it statically.
	public Text scoreText;                      //A reference to the UI text component that displays the player's score.
	public Text LivesText;						//A reference to the UI text element number of remaining Lives/Life Powerups
	public GameObject gameOvertext;				//A reference to the object that displays the text which appears when the player dies.

	private int score = 0;						//The player's score.
	public bool gameOver = true;				//Is the game over?
	public float scrollSpeed = -0.75f;          //Speed with which the obstacles and background move
	public int LifeCount = 1;					//How many lives does our player have?


	public GameObject startMenuUI;				//Reference to the Menu UI (When the application is started)
	public GameObject gameOverUI;               //Reference to the Game Over UI (When the bird crashes on obstacles or ground)
	public GameObject countdownUI;              //Reference to the Countdown UI (When the user taps play button, game countdown starts)

	private FirebaseFirestore db;               //Reference to the database in our Firebase cloud

	//Sounds for different events of the game, as the names suggest.
	public AudioSource dieSound;
	public AudioSource hitSound;
	public AudioSource powerUpSound;
	public AudioSource scoreSound;
	public AudioSource swooshSound;
	public AudioSource tapSound;

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

		//get a reference to Firestore cloud service to utilise the Firebase database capabilites.
		db = FirebaseFirestore.DefaultInstance;
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

	public int HighScore;

	/// <summary>
	/// Function to be registered as an event when the bird dies, crashing into obstacles or ground.
	/// The event is registered in Bird script.
	/// </summary>
	public void BirdDied()
	{
		gameOver = true;

		//Retrieve the collection named 'High Score Collection' from our database in the Firebase Cloud
		CollectionReference HighScoreCollection = db.Collection("High Score Collection");

		HighScore = 0;

		//Retrieve the results from the cloud
		HighScoreCollection.GetSnapshotAsync().ContinueWithOnMainThread(task =>
		{
			QuerySnapshot snapshot = task.Result;
			//Convert the queury's result from the server to List for convenience
			List<DocumentSnapshot> ListOfDocs = snapshot.ToList();

			//Since there is only one Document in the the collection, 'High Score Document'
			Dictionary<string, object> documentDictionary = ListOfDocs[0].ToDictionary();

			//Check for the field containing our high score
			if (documentDictionary.ContainsKey("High Score"))
			{
				if (int.TryParse(documentDictionary["High Score"].ToString(), out HighScore))
				{
					//if the current score of the user is higher than the already registered high score, then update it on the cloud
					if (score > HighScore)
					{
						SetHighScore(score);
					}

					else
					{
						SetPageState(UserInterfaceState.GameOver);
					}
				}
			}
		});
	}

	/// <summary>
    /// Updates the high score on the firebase cloud
    /// </summary>
    /// <param name="score">The score</param>
	public void SetHighScore (int score)
	{
		//Reference to the High Score Document stored in the cloud
		DocumentReference docRef = db.Collection("High Score Collection").Document("High Score Document");

		//Make the key value pair for our high score to be updated
		Dictionary<string, object> user = new Dictionary<string, object>
		{
				{ "High Score", score }
		};

		//Update the high score on the cloud and show the Game Over UI after the queury is executed
		docRef.SetAsync(user).ContinueWithOnMainThread(task => {
			HighScore = score;
			SetPageState(UserInterfaceState.GameOver);
		});
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
		LivesText.text = "Lives: 1";
		LifeCount = 1;
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

	/// <summary>
    /// Update the Lives or Life Count of the bird.
    /// Called when bird gets a Life Power up or crashes on the ground/obstacle.
    /// </summary>
    /// <param name="Lives">Lives to be updated. +1 for Power up, -1 for crashing into obstacle</param>
    public void UpdateLives(int Lives)
    {
		LifeCount += Lives;
		LivesText.text = "Lives: " + LifeCount;
    }
}
