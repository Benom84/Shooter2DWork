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
		int jumpType = 0;
		
		public BouncerNode(GameObject bouncer)	{this.bouncer = bouncer;}
		public GameObject getBouncer(){return bouncer;}
		public void setFather(BouncerNode father){this.father = father;}
		public BouncerNode getFather (){return father;}
		public void setLabel(int label){this.label = label;}
		public int getLabel (){return label;}
		public void setJumpType(int jump){this.jumpType = jump;}
		public int getJumpType (){return jumpType;}
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
	private bool left;					// If the enemy is facing left
	private Transform groundCheck;
	private int ground;					// The current ground's uid
	private GameObject playerScript;
	private int playerGround;			// The player's current ground
	private bool grounded;				// To check that the enemy is on the ground
	private GameObject mapManager;		// The map
	private bool isJumping = false;			// If the enemy is in the middle of a jump
	private BouncerNode nextBouncerNode;		//The node of the next bouncer the enemy is going towards

	private GameObject[] bouncers;		// List of all Bouncers
	private int[,] edgeMatrix;			// The edge matrix



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

		isJumping = true;

		if (rigidbody2D.velocity.x < 0)
				left = true;
	}


	void FixedUpdate ()
	{
		Collider2D[] frontHits = Physics2D.OverlapPointAll (frontCheck.position, 1);
		grounded = Physics2D.Linecast (transform.position, groundCheck.position, 1 << LayerMask.NameToLayer ("Ground"));

		if (grounded) 
			ground = Physics2D.Linecast (transform.position, groundCheck.position, 1 << LayerMask.NameToLayer ("Ground")).collider.gameObject.GetInstanceID ();

		if (player != null) {

				playerGround = player.GetComponent <PlayerControl> ().currentGround;

				// If ((we are not at the player's ground and the player changed location) or (our ground is not the one we wanted)) and grounded
				if ((rigidbody2D.velocity.y == 0) && isJumping && grounded && (ground != playerGround)) {
						nextBouncerNode = getNextBounderNode ();
						xDestination = nextBouncerNode.getBouncer ().transform.position.x;
						isJumping = false;
				} else if (ground == playerGround) {
						xDestination = player.transform.position.x;

				}
		}




		if (transform.rigidbody2D.velocity.y == 0)
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
		
		
		if ((nextBouncerNode != null) && (hit.gameObject == nextBouncerNode.getBouncer()) && (grounded)) {
			Jump();
		}
		
	}

	private void Jump()
	{
		Debug.Log("I should jump " + nextBouncerNode.getJumpType() + " the bouncer is " + nextBouncerNode.getBouncer().GetInstanceID()	);
		int jumpType = nextBouncerNode.getJumpType ();
		int jumpHorizontalDirection = jumpType % 10;
		int jumpVerticalDirection = jumpType / 10;

		if ((jumpHorizontalDirection == 0) && !left)
			Flip ();
		else if ((jumpHorizontalDirection == 1) && left)
		          Flip();

		if (jumpVerticalDirection == 1)
			// Add a vertical and horizontal force to the enemy.
			rigidbody2D.AddForce(new Vector2(jumpForceHor * (left ? -1 : 1), jumpForce));
		else
			rigidbody2D.AddForce(new Vector2(jumpForceHor * (left ? -1 : 1), jumpForce/10));

		isJumping = true;
	
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


	private BouncerNode getNextBounderNode()
	{
		Transform[] currGroundBouncersT = Physics2D.Linecast (transform.position, groundCheck.position, 1 << LayerMask.NameToLayer ("Ground")).collider.gameObject.GetComponentsInChildren<Transform> ();

		// Calculate the next bouncer to go to
		Queue<BouncerNode> q = new Queue<BouncerNode> ();
		BouncerNode[] bnArray = new BouncerNode[bouncers.Length];

		// Creating an array of nodes that only the the bouncers on our current ground are labeled 0
		foreach (GameObject currBouncer in bouncers) {
			int currIndex = indexOfBouncer(currBouncer);
			bnArray [currIndex] = new BouncerNode (currBouncer);
			foreach (Transform bouncerT in currGroundBouncersT)
			{
				if (currBouncer == bouncerT.gameObject) 
				{

					bnArray[currIndex].setLabel(0);
					q.Enqueue(bnArray[currIndex]);
				}
			}

		}

		// Getting the first node from the queue
		BouncerNode parentNode = q.Dequeue ();
		int label = parentNode.getLabel();

		// While queue is not empty
		while (parentNode != null) {
			for (int i = 0; i < bnArray.Length; i++) {
				// If there is an edge from parent to node i and node i was not marked
				if (edgeMatrix [indexOfBouncer (parentNode.getBouncer ()), i] > 0) {
					if (bnArray [i].getLabel () < 0) {
						
						q.Enqueue (bnArray [i]);
						bnArray [i].setLabel (label + 1);
						bnArray [i].setFather (parentNode);
					}
				}
			}
			
			label++;
			if (q.Count > 0) {
				parentNode = q.Dequeue ();
				label = parentNode.getLabel();
			}
			else
				parentNode = null;
		}
		// Now we have an array with nodes containing the label and father for each bouncer
		// Get the index of the bouncer closest to the player and set destination as that bouncer
		int playerBouncerIndex = indexOfBouncer (mapManager.GetComponent<MapCreator>().getPlayerClosestBouncer ());
		BouncerNode destination = bnArray [playerBouncerIndex];
		while (destination.getFather() != null) 
		{
			destination.getFather().setJumpType(edgeMatrix [indexOfBouncer (destination.getFather().getBouncer ()), indexOfBouncer (destination.getBouncer ())]);
			destination = destination.getFather ();
			
		}
		return destination;
	}

	private int indexOfBouncer(GameObject bouncer)
	{
		
		
		for (int i = 0; i < bouncers.Length; i++)
		{
			if (bouncer == bouncers[i])
				return i;
		}
		
		return -1;
	}
}
