using UnityEngine;
using UnityEngine.SceneManagement;

public class Bird : MonoBehaviour 
{
	public delegate void BirdDelegate();
	public static event BirdDelegate OnBirdDied; //Event that triggers when the bird dies.
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

	public Vector3 startPos;

	void Start()
	{
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
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		/*anim.SetTrigger("Restart");
		transform.localPosition = startPos;
		transform.rotation = Quaternion.identity;
		rb2d.simulated = false;
		isDead = true;
		GameControl.instance.gameOver = false;*/
	}

	/// <summary>
	/// Collision event for the bird
	/// </summary>
	void OnCollisionEnter2D(Collision2D other)
	{
		// Zero out the bird's velocity
		rb2d.velocity = Vector2.zero;
		// If the bird collides with something set it to dead...
		isDead = true;
		//...tell the Animator about it...
		anim.SetTrigger ("Die");
		//...and tell the game control about it.

		OnBirdDied();
	}
}
