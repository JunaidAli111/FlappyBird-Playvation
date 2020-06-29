using UnityEngine;
using UnityEngine.SceneManagement;

public class Bird : MonoBehaviour 
{
	public delegate void BirdDelegate();
	public static event BirdDelegate OnBirdDied; //Event that triggers when the bird dies.
	public static event BirdDelegate OnBirdCrashed; // Event that triggers when bird crashes on ground or obstacle but has more than one lives
	public static event BirdDelegate OnBirdScored; //Event that triggers when the game a bird passes through one obstacle.

	public float upForce;                   //Upward force of the "flap".
	public float tiltSmooth;				//Factor that will allow smooth transition towards bird facing downward when the it freefalls.
	private bool isDead = true;			//Has the player collided with a wall? Or the gameplay hasn't started yet?

	private Animator anim;					//Reference to the Animator component.
	private Rigidbody2D rb2d;               //Holds a reference to the Rigidbody2D component of the bird.

	Quaternion downRotation;			//Rotation of the bird when its freefalling.
	Quaternion forwardRotation;			//Rotation of the bird when the user taps on the screen, making it face forward.

	//Sounds for different events of the game, as the names suggest.
	public AudioSource tapSound;
	public AudioSource scoreSound;
	public AudioSource dieSound;

	public Vector3 startPos;			//Initial position of the bird 

	private bool CollisionOccured = false;		//This boolean with force the detection of only first collision in case of jittery collisions

	void Start()
	{
		Time.timeScale = 1f;
		//Get reference to the Animator component attached to this GameObject.
		anim = GetComponent<Animator> ();
		//Get and store a reference to the Rigidbody2D attached to this GameObject.
		rb2d = GetComponent<Rigidbody2D>();
		//Rotation values for the bird when the bird is falling down freely.
		downRotation = Quaternion.Euler(0f, 0f, -90f);
		//Rotation values for the bird when the user taps on the screen.
		forwardRotation = Quaternion.Euler(0f, 0f, 40f);
		//All the applications of physics will not be applicable on the bird until the game starts.
		rb2d.simulated = false;
	}

	void Update()
	{

		//Don't allow control if the bird has died.
		if (isDead) { return; }
		
		//Look for input to trigger a "flap".
		if (Input.GetMouseButtonDown(0)) 
		{
			//...tell the animator about it and then...
			anim.SetTrigger("Flap");
			//...zero out the birds current y velocity before...
			rb2d.velocity = Vector2.zero;
			//	new Vector2(rb2d.velocity.x, 0);
			//Face up the bird
			transform.rotation = forwardRotation;
			//rb2d.AddForce(new Vector2(0, upForce));

			//..giving the bird some upward force.
			rb2d.AddForce(Vector2.up * upForce, ForceMode2D.Force);
			// PLAY LATER tapSound.Play();
		}

		//Slowly transitioning the bird facing forward to bird facing downward using Lerp method to give it a natural feel
		transform.rotation = Quaternion.Lerp(transform.rotation, downRotation, tiltSmooth * Time.deltaTime);
		
	}


	/// <summary>
	/// Subscribe to events when the game object becomes active.
	/// </summary>
	void OnEnable()
	{
		GameControl.OnGameStarted += OnGameStarted;
		GameControl.OnGameOverConfirmed += OnGameOverConfirmed;
	}


	/// <summary>
	/// Unsubscribe to events when the game object becomes inactive.
	/// </summary>
	void OnDisable()
	{
		GameControl.OnGameStarted -= OnGameStarted;
		GameControl.OnGameOverConfirmed -= OnGameOverConfirmed;
	}

	/// <summary>
    /// Event that triggers when the game starts, allowing the physics to be applied on the bird.
    /// </summary>
	void OnGameStarted()
	{
		// Zero out the bird's velocity
		rb2d.velocity = Vector3.zero;
		//Enables the physics operations on the bird.
		rb2d.simulated = true;
		//Bird is alive when the game starts
		isDead = false;
	}


	/// <summary>
	/// Event that is triggered to reset all the fields and values and brings the game to its initial state.
	/// </summary>
	void OnGameOverConfirmed()
	{
		//Reset the sprite of bird from dead to alive
		anim.SetTrigger("Restart");
		//Set the birds position to its inital position
		transform.localPosition = startPos;
		//Set the birds rotation to its inital rotation
		transform.rotation = Quaternion.identity;
		//Physics operations will not work on the bird until the user restarts the gameplay
		rb2d.simulated = false;
		//Bird is assumed to be dead until the game play starts
		isDead = true;
		//Allow the detection of the first collision in OnCollisionEnter2D() event again
		CollisionOccured = false;
		//
		GameControl.instance.gameOver = true;
	}

	/// <summary>
	/// Collision event for the bird
	/// </summary>
	void OnCollisionEnter2D(Collision2D other)
	{
		//If the bird is having jittery collisions, so to avoid calling the OnCollisionEnter2D again and again
		if (CollisionOccured == true) return;

		//Negative one life for the bird
		GameControl.instance.UpdateLives(-1);
		//First collision detected, shouldn't be called again for jittery collisions
		CollisionOccured = true;

		//If the life count of the bird is zero, game over path is followed
		if (GameControl.instance.LifeCount == 0)
		{
			// If the bird collides with something set it to dead...
			isDead = true;
			// Zero out the bird's velocity
			rb2d.velocity = Vector2.zero;
			//...tell the Animator about it...
			anim.SetTrigger("Die");
			//...and tell the game control about it.

			OnBirdDied();
		}

		//If the bird has more than one lives, then the game is reset and the user keeps playing the game
		//basically the game quickly resets, without any pause
		else
		{
			// Zero out the bird's velocity
			rb2d.velocity = Vector2.zero;
			//Set the birds position to its inital position
			transform.localPosition = startPos;
			//Set the birds rotation to its inital rotation
			transform.rotation = Quaternion.identity;
			OnBirdCrashed();
			//Detect collisions again :)
			CollisionOccured = false;

		}
	}

}
