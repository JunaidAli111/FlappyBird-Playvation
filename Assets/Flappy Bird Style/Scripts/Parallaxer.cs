using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Parallaxer class is used for all the objects that needs to be parallaxed (Ground, clouds, and the obstacles).
/// </summary>
public class Parallaxer : MonoBehaviour
{
	/// <summary>
    /// Pool of objects that are initialised at the start of the game and reused as the game progresses.
    /// Its advantage is that you don't need to allocate and destroy objects at runtime, which decreases the performance.
    /// </summary>
	class PoolObject
	{
		public Transform transform;								//Transform of the pooled object
		public bool inUse;										//If the pooled object is in use at the moment
		public PoolObject(Transform t) { transform = t; }		//Constructer
		public void Use() { inUse = true; }						//If the pooled object is being used
		public void Dispose() { inUse = false; }				//If the usage of current pooled object is finished
	}


	/// <summary>
    /// The Y Spawn range is basically for obstacles so they can places randomly on different Y axis values to create a better gameplay.
    /// </summary>
	[System.Serializable]
	public struct YSpawnRange
	{
		public float minY;
		public float maxY;
	}


	public GameObject Prefab;	//The game object that needs to be pooled.
	public int poolSize;		//How many objects in the pool on the standby.
	public float shiftSpeed;	//The speed at which objects move on the x axis.
	public float spawnRate;		//How often objects spawn.

	public YSpawnRange ySpawnRange;		//As mentioned earlier the range between the obstacles will be places randomly in the game.
	public Vector3 defaultSpawnPos;		//Position where all the pooled objects will be placed on spawning.
	public bool spawnImmediate;			//Does the object needs to be spawned right away? (Such as clouds)
	public Vector3 immediateSpawnPos;	//The position of the first pooled object which needs to be spawned immediately
	public Vector2 targetAspectRatio;	//Aspect ratio need to adjust the spawning positions for different devices

	float spawnTimer;					//Float value to count till Spawn Rate.
	PoolObject[] poolObjects;			//Array of all the pooled objects
	float targetAspect;                 //targetAspectRatio.x / targetAspectRatio.y
	GameControl game;					//Reference to the Game Control object

	void Awake()
	{
		Configure();
	}

	void Start()
	{
		game = GameControl.instance;
	}

	void OnEnable()
	{
		GameControl.OnGameOverConfirmed += OnGameOverConfirmed;
	}

	void OnDisable()
	{
		GameControl.OnGameOverConfirmed -= OnGameOverConfirmed;
	}

	/// <summary>
    /// Event called when the user presses the replay button after the bird dies
    /// Resets the intial state of all the pooled objects
    /// </summary>
	void OnGameOverConfirmed()
	{
		for (int i = 0; i < poolObjects.Length; i++)
		{
			poolObjects[i].Dispose();
			poolObjects[i].transform.position = Vector3.one * 1000;
		}
		Configure();
	}

	//Translate the position of all the pooled objects
	void Update()
	{
		if (game.gameOver) return;

		Shift();
		spawnTimer += Time.deltaTime;
		if (spawnTimer > spawnRate)
		{
			Spawn();
			spawnTimer = 0;
		}
	}

	/// <summary>
	/// Allocate the pool objects according to the pool size and initialise them as unused
	/// </summary>
	void Configure()
	{
		//spawning pool objects
		targetAspect = targetAspectRatio.x / targetAspectRatio.y;
		poolObjects = new PoolObject[poolSize];
		for (int i = 0; i < poolObjects.Length; i++)
		{
			GameObject go = Instantiate(Prefab);
			Transform t = go.transform;
			t.SetParent(transform);					//The parent object with the Paraller Script Component
			t.position = Vector3.one * 1000;		//At first initialise the project out of the visual Bounds, from where they can be spawned to required position
			poolObjects[i] = new PoolObject(t);		
			poolObjects[i].Dispose();
		}


		if (spawnImmediate)
		{
			SpawnImmediate();
		}
	}


	/// <summary>
    /// Spawn the pooled object at the specified location
    /// </summary>
	void Spawn()
	{
		//moving pool objects into place
		Transform t = GetPoolObject();
		if (t == null) return;
		Vector3 pos = Vector3.zero;
		pos.y = Random.Range(ySpawnRange.minY, ySpawnRange.maxY);
		pos.x = (defaultSpawnPos.x * Camera.main.aspect) / targetAspect;
		t.localPosition = pos;
	}


	/// <summary>
    /// The parallaxed objects that need the first object to be spawned immediately
    /// such as clouds and ground
    /// so they can be visible as soon as the application starts
    /// unlike Obstacles which will instantiate off the screen and will be shifted towards the visual bounds slowly
    /// </summary>
	void SpawnImmediate()
	{
		Transform t = GetPoolObject();
		if (t == null) return;
		Vector3 pos = Vector3.zero;
		pos.y = Random.Range(ySpawnRange.minY, ySpawnRange.maxY);
		pos.x = (immediateSpawnPos.x * Camera.main.aspect) / targetAspect;
		t.localPosition = pos;
		Spawn();
	}

	/// <summary>
	/// loop through pool objects 
	/// moving them
	/// discarding them as they go off screen 
	/// </summary>
	void Shift()
	{
		for (int i = 0; i < poolObjects.Length; i++)
		{
			poolObjects[i].transform.position += Vector3.left * shiftSpeed * Time.deltaTime;
			CheckDisposeObject(poolObjects[i]);
		}
	}

	/// <summary>
    /// Check if the parallaxed object has gone out of the screen
    /// </summary>
	void CheckDisposeObject(PoolObject poolObject)
	{
		if (poolObject.transform.position.x < (-defaultSpawnPos.x * Camera.main.aspect) / targetAspect)
		{
			poolObject.Dispose(); //Make sure the object is disposed and marked as unused
			poolObject.transform.position = Vector3.one * 1000; //Put it way off screen
		}
	}

	/// <summary>
	/// Retrieving first available pool object
	/// </summary>
	/// <returns>Transform of the first available object</returns>
	Transform GetPoolObject()
	{

		for (int i = 0; i < poolObjects.Length; i++)
		{
			if (!poolObjects[i].inUse)
			{
				poolObjects[i].Use();
				return poolObjects[i].transform;
			}
		}
		return null;
	}

}
