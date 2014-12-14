using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class MapCreator : MonoBehaviour {

	[HideInInspector]
	public class BouncerNode
	{
		GameObject bouncer = null;
		BouncerNode father = null;
		int label = -1;

		public BouncerNode(GameObject bouncer)
		{
			this.bouncer = bouncer;

		}

		public GameObject getBouncer()
		{
			return bouncer;
		}

		public void setFather(BouncerNode father)
		{
			this.father = father;
		}

		public BouncerNode getFather ()
		{
			return father;
		}

		public void setLabel(int label)
		{
			this.label = label;	
		}

		public int getLabel ()
		{
			return label;	
		}

	
	}

	[HideInInspector]
	public GameObject[] bouncers;
	public float maxUpJump = 5;
	public float maxDownJump = 8;
	public float surfaceChange = 0.4f;
	[HideInInspector]
	public int[,] edgeMatrix;

	// Use this for initialization
	void Start () {
	
		bouncers = GameObject.FindGameObjectsWithTag ("Bouncer");
		edgeMatrix = new int[bouncers.Length, bouncers.Length];
		Debug.Log ("Edge Matrix: " + edgeMatrix.Length);

		int counter = 0;
		int row = 0;


		// Create a bouncer graph
		foreach (GameObject bouncer in bouncers) 
		{

			int col = 0;
			// The first bouncer in each list is the bouncer from which the links go

			foreach (GameObject otherBouncer in bouncers)
			{

				edgeMatrix[row, col] = 0;
				if (bouncer == otherBouncer) 
				{
					col++;
					continue;
				}
				float distance = Vector2.Distance(bouncer.transform.position, otherBouncer.transform.position);

				// Check if the other bouncer is higher than this
				bool higher = (otherBouncer.transform.position.y > (bouncer.transform.position.y + surfaceChange));

				if (higher) 
				{
					if (distance < maxUpJump) 
					{
						Debug.Log("Added an edge between: " + bouncer.GetInstanceID() + " and " + otherBouncer.GetInstanceID());
						edgeMatrix[row, col] = 2;
					}
				} else
				{
					if (distance < maxDownJump)
					{
						Debug.Log("Added an edge between: " + bouncer.GetInstanceID() + " and " + otherBouncer.GetInstanceID());
						edgeMatrix[row, col] = 1;
					}
				}

				col++;
			
			}
			row++;
			counter++;
		}


	}
	

	public int indexOfBouncer(GameObject bouncer)
	{


		int index = 0;
		for (int i = 0; i < bouncers.Length; i++)
		{
			if (bouncer == bouncers[i])
				return i;
		}

		return 0;
	}

	public GameObject getPlayerClosestBouncer()
	{
		GameObject player = GameObject.FindGameObjectWithTag ("Player");
		GameObject playerGround = player.GetComponent <PlayerControl> ().currentGroundObject;
		Transform[] groundBouncers = playerGround.GetComponentsInChildren<Transform>();

		GameObject result = null;
		float distance = float.MaxValue;
		foreach (Transform transformBouncer in groundBouncers) 
		{
			if (transformBouncer.tag == "Bouncer") 
			{
				if ((Vector2.Distance(transformBouncer.position, player.transform.position)) < distance)
					result = transformBouncer.gameObject;
			}
		}

		return result;
	}
}