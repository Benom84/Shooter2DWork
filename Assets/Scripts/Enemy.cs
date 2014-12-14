using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{

	public class BouncerNode
	{
		GameObject bouncer = null;
		BouncerNode father = null;
		int label = -1;
		
		public BouncerNode(GameObject bouncer)	{this.bouncer = bouncer;}
		public GameObject getBouncer(){return bouncer;}
		public void setFather(BouncerNode father){this.father = father;}
		public BouncerNode getFather (){return father;}
		public void setLabel(int label){this.label = label;}
		public int getLabel (){return label;}
	}





	public float moveSpeed = 2f;		// The speed the enemy moves at.
	public int HP = 2;					// How many times the enemy can be hit before it dies.
	public Sprite deadEnemy;			// A sprite of the enemy when it's dead.
	public Sprite damagedEnemy;			// An optional sprite of the enemy when it's damaged.
	public AudioClip[] deathClips;		// An array of audioclips that can play when the enemy dies.
	public GameObject hundredPointsUI;	// A prefab of 100 that appears when the enemy dies.
	public float deathSpinMin = -100f;	// A value to give the minimum amount of Torque when dying
	public float deathSpinMax = 100f;	// A value to give the maximum amount of Torque when dying
	public float jumpForce = 10f;
	public float jumpForceHor = 10f;


	private SpriteRenderer ren;			// Reference to the sprite renderer.
	private Transform frontCheck;		// Reference to the position of the gameobject used for checking if something is in front.
	private bool dead = false;			// Whether or not the enemy is dead.
	private Score score;				// Reference to the Score script.


	private GameObject player;			// Reference to the player object
	private float xDestination;			// The destination on the X axis
	private float yDestination;			// The destination on the Y axis
	private bool left;					// If the enemy is facing left
	private Transform groundCheck;
	private int ground;
	private GameObject playerScript;
	private int playerGround;			// The player's current ground
	private bool grounded;
	private GameObject mapManager;		// The map
	private bool jump = false;
	private GameObject nextBouncer;		//

	private int oldGround;

	private GameObject[] bouncers;		// List of all Bouncers
	private int[,] edgeMatrix;			// The edge matrix
	private int prevPlayerGround; //
	private Stack<GameObject> path;



	void Awake ()
	{
		// Setting up the references.
		ren = transform.Find ("body").GetComponent<SpriteRenderer> ();
		frontCheck = transform.Find ("frontCheck").transform;
		score = GameObject.Find ("Score").GetComponent<Score> ();

		groundCheck = transform.Find ("groundCheck").transform;
		player = GameObject.FindGameObjectWithTag ("Player");



		mapManager = GameObject.FindGameObjectWithTag ("map");
		bouncers = mapManager.GetComponent<MapCreator> ().bouncers;
		edgeMatrix = mapManager.GetComponent<MapCreator> ().edgeMatrix;


		if (rigidbody2D.velocity.x < 0)
				left = true;
	}

	void FixedUpdate ()
	{
		// Create an array of all the colliders in front of the enemy.
		Collider2D[] frontHits = Physics2D.OverlapPointAll (frontCheck.position, 1);

		// If the enemy is grounded and marked as jump, then jump
		if((jump) && (grounded))
		{
			
			// Add a vertical and horizontal force to the enemy.
			rigidbody2D.AddForce(new Vector2(jumpForceHor * (left ? 1 : -1), jumpForce));
			

		}

		//Check if the enemy is on the ground, if it is, get the ground id and mark it
		grounded = Physics2D.Linecast (transform.position, groundCheck.position, 1 << LayerMask.NameToLayer ("Ground"));
		if (grounded) 
		{
			ground = Physics2D.Linecast (transform.position, groundCheck.position, 1 << LayerMask.NameToLayer ("Ground")).collider.gameObject.GetInstanceID ();
			jump = false;

		}

		// If the player is not dead and the enemy is not jumping we have to know where he is going
		if ((player != null) && (!jump)) {

			// Getting the player ground
			playerGround = player.GetComponent <PlayerControl> ().currentGround;

			// If our ground is not the same as the player's and the player switched grounds since last update
			if ((ground != playerGround) && (prevPlayerGround != playerGround))
			{
				Debug.Log ("FixedUpdate 2.5");
				// save the new player ground and get the position x of the next bouncer to go to
				prevPlayerGround = playerGround;
				nextBouncer = getNextBouncer ();
				xDestination = nextBouncer.transform.position.x;
				yDestination = nextBouncer.transform.position.y;
				Debug.Log("xDestination is: " + xDestination);
			}
			else if (ground == playerGround)
			{
				// If the ground is the player's - go towards him
				xDestination = player.transform.position.x;
				yDestination = player.transform.position.y;
				nextBouncer = null;
				path = null;
			}
			
		}
		//Checking if the enemy should flip according to his destination
		if (grounded)
			if (((transform.position.x - xDestination < 0) && left) || 
		    	((transform.position.x - xDestination > 0) && !left))
				Flip ();
		


		// Check each of the colliders.
		foreach (Collider2D c in frontHits) {
				// If any of the colliders is an Obstacle...
				if (c.tag == "Obstacle") {
						// ... Flip the enemy and stop checking the other colliders.
						Flip ();
						break;
				}
		}



		// Set the enemy's velocity to moveSpeed in the x direction.
		// Only if we are not on the destination itself.
		if (Mathf.Abs(xDestination - transform.position.x) > 0.1)
			rigidbody2D.velocity = new Vector2 (transform.localScale.x * moveSpeed, rigidbody2D.velocity.y);	

		// If the enemy has one hit point left and has a damagedEnemy sprite...
		if (HP == 1 && damagedEnemy != null)
		// ... set the sprite renderer's sprite to be the damagedEnemy sprite.
			ren.sprite = damagedEnemy;
		
		// If the enemy has zero or fewer hit points and isn't dead yet...
		if (HP <= 0 && !dead)
		// ... call the death function.
				Death ();
	}

	void OnTriggerStay2D(Collider2D hit)
	{
		
		
		if ((hit.gameObject == nextBouncer) && (grounded)) {
			jump = true;
			Debug.Log("Touched bouncer");
		}
		
	}


	public void Hurt ()
	{
			// Reduce the number of hit points by one.
			HP--;
	}

	void Death ()
	{
			// Find all of the sprite renderers on this object and it's children.
			SpriteRenderer[] otherRenderers = GetComponentsInChildren<SpriteRenderer> ();

			// Disable all of them sprite renderers.
			foreach (SpriteRenderer s in otherRenderers) {
					s.enabled = false;
			}

			// Re-enable the main sprite renderer and set it's sprite to the deadEnemy sprite.
			ren.enabled = true;
			ren.sprite = deadEnemy;

			// Increase the score by 100 points
			score.score += 100;

			// Set dead to true.
			dead = true;

			// Allow the enemy to rotate and spin it by adding a torque.
			rigidbody2D.fixedAngle = false;
			rigidbody2D.AddTorque (Random.Range (deathSpinMin, deathSpinMax));

			// Find all of the colliders on the gameobject and set them all to be triggers.
			Collider2D[] cols = GetComponents<Collider2D> ();
			foreach (Collider2D c in cols) {
					c.isTrigger = true;
			}

			// Play a random audioclip from the deathClips array.
			int i = Random.Range (0, deathClips.Length);
			AudioSource.PlayClipAtPoint (deathClips [i], transform.position);

			// Create a vector that is just above the enemy.
			Vector3 scorePos;
			scorePos = transform.position;
			scorePos.y += 1.5f;

			// Instantiate the 100 points prefab at this point.
			Instantiate (hundredPointsUI, scorePos, Quaternion.identity);
	}

	public void Flip ()
	{
			// Multiply the x component of localScale by -1.
			Vector3 enemyScale = transform.localScale;
			enemyScale.x *= -1;
			transform.localScale = enemyScale;
			left = !(left);
	}




	private void createPath()
	{

	
	}

	private GameObject getNextBouncer() {


		GameObject source = null;
		float distance = float.MaxValue;
		
		// Find the closest bouncer to the enemey - should be modified to the closest on the same ground object
		foreach (GameObject bouncer in bouncers) 
		{
			if (Vector2.Distance(bouncer.transform.position, transform.position) < distance)
			{
				distance = Vector2.Distance(bouncer.transform.position, transform.position);
				source = bouncer;
			}
		}

		// Calculate the next bouncer to go to
		Debug.Log ("The source determined as X: " + source.transform.position.x + " Y: " + source.transform.position.y);
		return shortestPathToPlayer(source);
		
	}



	private GameObject shortestPathToPlayer(GameObject source)
	{
		Queue<BouncerNode> q = new Queue<BouncerNode> ();
		BouncerNode[] bnArray = new BouncerNode[bouncers.Length];
		int counter = 0;


		// Creating an array of nodes that only the source is labeled 0
		foreach (GameObject currBouncer in bouncers) {
			bnArray [counter] = new BouncerNode (currBouncer);
			if (currBouncer == source) 
			{
				bnArray [counter].setLabel (0);
				q.Enqueue (bnArray [counter]);
			}
			counter++;
		}


		// Getting the source node from the queue
		BouncerNode parentNode = q.Dequeue ();
		int label = 0;

		// While queue is not empty
		while (parentNode != null) {
			for (int i = 0; i < bnArray.Length; i++) {
				// If there is an edge from parent to node i and node i was not marked
				//Debug.Log("Checking bouncer: " + bnArray[i].getBouncer().GetInstanceID() + " For parent: " + parentNode.getBouncer().GetInstanceID());
				if (edgeMatrix [mapManager.GetComponent<MapCreator> ().indexOfBouncer (parentNode.getBouncer ()), i] > 0) {
					if (bnArray [i].getLabel () < 0) {
						q.Enqueue (bnArray [i]);
						bnArray [i].setLabel (label + 1);
						//Debug.Log("Enqueueing " + bnArray[i].getBouncer().GetInstanceID() + " With parent: " + parentNode.getBouncer().GetInstanceID());
						bnArray [i].setFather (parentNode);
					}
				}
			}

			label++;
			if (q.Count > 0)
				parentNode = q.Dequeue ();
			else
				parentNode = null;
		}
		// Now we have an array with nodes containing the label and father for each bouncer

		// Get the index of the bouncer closest to the player and set destination as that bouncer
		int playerBouncerIndex = mapManager.GetComponent<MapCreator>().indexOfBouncer (mapManager.GetComponent<MapCreator>().getPlayerClosestBouncer ());
		BouncerNode destination = bnArray [playerBouncerIndex];
		//Debug.Log("Player closest bouncer is " + destination.getBouncer().GetInstanceID() );

		// get the child of the source that will lead to the shortest path to the destination
		//Debug.Log ("First: " + (destination.getFather () != null));
		//Debug.Log("Second: " + (destination.getFather().getBouncer() != source));
		while (destination.getFather() != null) 
		{
			destination = destination.getFather ();
			//Debug.Log("Its parent bouncer is X: " + destination.getBouncer().transform.position.x + " Y: " + destination.getBouncer().transform.position.y );
		}
		return destination.getBouncer();


	}

	
}
