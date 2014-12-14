using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class MapCreator : MonoBehaviour {

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


	public GameObject[] bouncers;
	public float maxUpJump = 5;
	public float maxDownJump = 8;
	public float surfaceChange = 0.4f;


	private List<BouncerNode>[] bouncerMap;
	public int[,] edgeMatrix;

	// Use this for initialization
	void Start () {
	
		bouncers = GameObject.FindGameObjectsWithTag ("Bouncer");
		edgeMatrix = new int[bouncers.Length, bouncers.Length];
		Debug.Log ("Edge Matrix: " + edgeMatrix.Length);
		bouncerMap = new List<BouncerNode>[bouncers.Length];
		int counter = 0;
		int row = 0;


		// Create a bouncer graph
		foreach (GameObject bouncer in bouncers) 
		{
			bouncerMap[counter] = new List<BouncerNode>();
			int col = 0;
			// The first bouncer in each list is the bouncer from which the links go
			bouncerMap[counter].Insert(0, new BouncerNode(bouncer));
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
						bouncerMap[counter].Add(new BouncerNode(otherBouncer));
						Debug.Log("Added an edge between: " + bouncer.GetInstanceID() + " and " + otherBouncer.GetInstanceID());
						edgeMatrix[row, col] = 1;
					}
				} else
				{
					if (distance < maxDownJump)
					{
						bouncerMap[counter].Add(new BouncerNode(otherBouncer));
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
	

	public GameObject getNextBouncer(Transform trans) {

		GameObject destination = null;
		float distance = float.MaxValue;

		foreach (GameObject bouncer in bouncers) 
		{
			if (Vector2.Distance(bouncer.transform.position, trans.position) < distance)
			{
				distance = Vector2.Distance(bouncer.transform.position, trans.position);
				destination = bouncer;
			}
		}

		return shortestPathToPlayer(destination);
	
	}

	private GameObject shortestPathToPlayer(GameObject source)
	{
		Queue<BouncerNode> q = new Queue<BouncerNode>();
		BouncerNode[] bnArray = new BouncerNode[bouncers.Length];
		int counter = 0;
		foreach (GameObject currBouncer in bouncers)
		{
			bnArray[counter] = new BouncerNode(currBouncer);
			if (currBouncer == source) 
			{
				bnArray[counter].setLabel(0);
				q.Enqueue(bnArray[counter]);
			}
			counter++;
		}

		BouncerNode parentNode = q.Dequeue();
		int label = 0;
		while (parentNode.getBouncer() != null) 
		{
			for (int i = 0; i < bnArray.Length; i++)
			{
				if (edgeMatrix[indexOfBouncer(parentNode.getBouncer()), i] == 1)
				{
					if (bnArray[i].getLabel() < 0) 
					{
						q.Enqueue(bnArray[i]);
						bnArray[i].setLabel(label + 1);
						bnArray[i].setFather(parentNode);
					}
				}
			}
			label++;
			parentNode = q.Dequeue();
		}
		// Now we have an array with nodes containing the label and father for each bouncer


		int playerBouncerIndex = indexOfBouncer (getPlayerClosestBouncer ());
		BouncerNode destination = bnArray [playerBouncerIndex];
		while ((destination.getFather() != null) && (destination.getFather().getBouncer() != source))
						destination = destination.getFather();


		return destination.getBouncer();
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