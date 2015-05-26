using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAgentController : MonoBehaviour {

	public bool lockedMovement, seek;
	public float turnSpeed, moveSpeed;
	public Vector3 myPosition, myHeading;

	public float thisDist, maxDistance;
	private Vector3 myDirection;
	private RaycastHit2D myHit;
	private List<RaycastHit2D> hitList; 
	private ArrayList idList;

	// Use this for initialization
	void Start () {
		lockedMovement = false;
		seek = false;
		turnSpeed = 200f;
		moveSpeed = 5f;
		maxDistance = 5f;
	}


	// Update is called once per frame
	void Update () {

		// Update heading and position
		myPosition = transform.position;
		myHeading = transform.up;

		// Check for key presses
		if (seek) {
			mySeek(Camera.main.ScreenToWorldPoint( new Vector3(Input.mousePosition.x, Input.mousePosition.y, 
			                                                   -Camera.main.transform.position.z) ));
		}
		else if (!lockedMovement) {
			checkForInput ();
		}
		else {
			moveTo(Camera.main.GetComponent<DebugListener>().target);
		}

	} // end of Update()


	// Check for valid key presses (W,S,A,D) and move or turn accordingly
	void checkForInput() {

		// this is how to move
		//transform.Translate(+/- Vector3.up * moveSpeed * Time.deltaTime);
		
		// Check for input: W = Forwards
		if (Input.GetKey (KeyCode.W)) {
			transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
		}
		
		// Check for input: S = Backwards
		if (Input.GetKey (KeyCode.S)) {
			transform.Translate(-Vector3.up * moveSpeed * Time.deltaTime);
		}

		// this is how to rotate in 2D
		//transform.Rotate(Vector3.forward, +/- turnSpeed * Time.deltaTime);
		
		// Check for input: A = Turn left
		if (Input.GetKey (KeyCode.A)) {
			transform.Rotate(Vector3.forward, turnSpeed * Time.deltaTime);
		}
		
		// Check for input: D = Turn right
		if (Input.GetKey (KeyCode.D)) {
			transform.Rotate(Vector3.forward, -turnSpeed * Time.deltaTime);
		}
	
	} // end of checkForInput()


	// Find the closest node to the player
	public int findClosestNode() {
		
		float yFactor, xFactor;
		float min;
		int mini;

		hitList = new List<RaycastHit2D>();
		idList = new ArrayList();
		
		collider2D.enabled = false;
		
		// Search 360 degrees around the player
		for (int i = 0; i < 360; i++) {
			
			// Change x and y factors
			yFactor = Mathf.Sin(i);
			xFactor = Mathf.Cos(i);
			
			// Turn ray by one degree
			myDirection = (yFactor * transform.up) + (xFactor * transform.right);
			myDirection.Normalize();
			
			// Draw the radar circle in the Scene for debugging
			myHit = Physics2D.Raycast(transform.position, myDirection, maxDistance);
			//Debug.DrawRay(transform.position, maxDistance * myDirection, Color.cyan);
			
			// If ray hit something, check if it is a new node, and if so add it
			if (myHit.collider != null && myHit.collider.gameObject.tag.Equals("Node")) {
				if (myHit.collider.gameObject.GetComponent<NodeController>().myID >= 0) {
					if (!hitList.Contains(myHit) && !idList.Contains(myHit.collider.gameObject.GetComponent<NodeController>().myID)) {
						hitList.Add( myHit );
						idList.Add(myHit.collider.gameObject.GetComponent<NodeController>().myID);
					}
				}
			} // end of outer if
		}
		
		collider2D.enabled = true;
		
		// If no nodes are close enough, return that information
		if (hitList.Count == 0) {
			return -1;
		}
		
		// Loop through each nearby node and find the one that is closest
		min = hitList[0].distance;
		mini = 0;
		for (int i = 0; i < hitList.Count; i++) {
			//Debug.Log("found: " + hitList[i].collider.gameObject.GetComponent<NodeController>().myID);
			//Debug.Log("at distance: " + hitList[i].distance);
			if (hitList[i].distance < min) {
				min = hitList[i].distance;
				mini = i;
			}
		}

		//Debug.Log("closest is " + mini + " = " + hitList[mini].collider.gameObject.GetComponent<NodeController>().myID );
		return hitList[mini].collider.gameObject.GetComponent<NodeController>().myID;
		
	} // end of findClosestNode()
	

	// Given two positions, find the angle from from to to
	public float angleBetween(Vector3 from, Vector3 to, int fix) {

		float angle = Mathf.DeltaAngle(Mathf.Atan2(from.y, from.x) * Mathf.Rad2Deg,
		                               Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg);
		
		if (fix == 1 && angle < 0f) {
			angle += 360f;
		}
		
		return angle;
	}


	// Move to the given position
	public void moveTo(Vector3 pos) {

		Vector3 moveDirection = pos - transform.position;
		moveDirection.z = 0;
		moveDirection.Normalize();
		//Debug.Log ("moveDirection = " + moveDirection);

		float angle = angleBetween(transform.up, moveDirection, 1);
		//Debug.Log ("angle = " + angle);

		if (angle > 10f) {
			if (angle > 180f) {
				transform.Rotate(Vector3.forward, -turnSpeed * Time.deltaTime);
			}
			else {
				transform.Rotate(Vector3.forward, turnSpeed * Time.deltaTime);
			}
		}
		//Debug.Log("distance = " + Vector3.Distance(pos, transform.position));
		if (Vector3.Distance(pos, transform.position) > .1f) {
			transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
		}
		else {
			Camera.main.GetComponent<DebugListener>().count++;
			if (pos == Camera.main.GetComponent<DebugListener>().nodesList[Camera.main.GetComponent<DebugListener>().nd].transform.position) {
				lockedMovement = false;
				Camera.main.GetComponent<DebugListener>().followPath = false;
				//Camera.main.GetComponent<DebugListener>().ns = -1;
			}
		}

	} // end of moveTo()


	// Move to mouse position in a seek behavior
	public void mySeek(Vector3 pos) {

		if (Vector3.Distance(pos, transform.position) > .1f) {
			Vector3 moveDirection = pos - transform.position;
			moveDirection.z = 0;
			moveDirection.Normalize();

			Vector3 target = moveDirection * moveSpeed + transform.position;
			transform.position = Vector3.Lerp (transform.position, target, Time.deltaTime);
			
			float targetAngle = Mathf.Atan2 (moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Slerp (transform.rotation, 
			                                       Quaternion.Euler (0, 0, targetAngle - 90f), 
			                                       turnSpeed * Time.deltaTime);
		}
	} // end of mySeek()
}
