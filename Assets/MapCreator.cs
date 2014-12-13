using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class MapCreator : MonoBehaviour {

	public class BouncerNode
	{
		GameObject bouncer = null;
		GameObject father = null;

		public BouncerNode(GameObject bouncer)
		{
			this.bouncer = bouncer;

		}

		public void setFather(GameObject father)
		{
			this.father = father;
		}

	
	}


	public GameObject[] bouncers;
	public float maxUpJump = 5;
	public float maxDownJump = 8;
	public float surfaceChange = 0.4f;


	private List<BouncerNode>[] bouncerMap;

	// Use this for initialization
	void Start () {
	
		bouncers = GameObject.FindGameObjectsWithTag ("Bouncer");
		bouncerMap = new List<BouncerNode>[bouncers.Length];
		int counter = 0;

		// Create a bouncer graph
		foreach (GameObject bouncer in bouncers) 
		{
			bouncerMap[counter] = new List<BouncerNode>();

			// The first bouncer in each list is the bouncer from which the links go
			bouncerMap[counter].Insert(0, new BouncerNode(bouncer));
			foreach (GameObject otherBouncer in bouncers)
			{
				if (bouncer == otherBouncer)
					continue;

				float distance = Vector2.Distance(bouncer.transform.position, otherBouncer.transform.position);

				// Check if the other bouncer is higher than this
				bool higher = (otherBouncer.transform.position.y > (bouncer.transform.position.y + surfaceChange));

				if (higher) 
				{
					if (distance < maxUpJump)
						bouncerMap[counter].Add(new BouncerNode(otherBouncer));
				} else
				{
					if (distance < maxDownJump)
						bouncerMap[counter].Add(new BouncerNode(otherBouncer));
				}
			
			}

			counter++;
		}


	}
	
	// Update is called once per frame
	void Update () {
	
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

		return destination;
	
	}
}