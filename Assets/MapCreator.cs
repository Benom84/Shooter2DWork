using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* Notice the coding: 
 * 10 - Jump up and left
 * 11 - Jump up and right
 * 20 - Jump down and left
 * 21 - Jump down and right
 *
 */
public class MapCreator : MonoBehaviour {

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
						if (bouncer.transform.position.x > otherBouncer.transform.position.x)
							edgeMatrix[row, col] = 10;
						else
							edgeMatrix[row, col] = 11;

						Debug.Log("Added an edge between: " + bouncer.GetInstanceID() + " and " + otherBouncer.GetInstanceID() + " Type: " + edgeMatrix[row, col]);

					}
				} else
				{
					if (distance < maxDownJump)
					{


						if (bouncer.transform.position.x > otherBouncer.transform.position.x)
							edgeMatrix[row, col] = 20;
						else
							edgeMatrix[row, col] = 21;

						Debug.Log("Added an edge between: " + bouncer.GetInstanceID() + " and " + otherBouncer.GetInstanceID() + " Type: " + edgeMatrix[row, col]);
					}
				}

				col++;
			
			}
			row++;
			counter++;
		}


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