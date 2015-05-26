using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class pathNode {

	public int id;				// id of this node
	public float cost;			// F = G+H
	public float travelled;		// G
	public float heuristic;		// H
	public List<int> breadcrumbs;	// List of nodes from source to destination

	public pathNode(int i, float g, float h, List<int> b) {
		id = i;
		travelled = g;
		heuristic = h;
		cost = travelled + heuristic;
		breadcrumbs = b;
	}

} // end of class pathNode

public class DebugListener : MonoBehaviour {

	public GameObject[] nodesList, player;
	public int ns, nd, count;
	public Vector3 target;
	public bool showFlip, showFlipLines, followPath;
	public ArrayList myQueue;
	public List<int> myBestPath;

	void Start() {
		ns = -1;
		nd = -1;
		showFlip = true;
		showFlipLines = true;
		followPath = false;
		nodesList = GameObject.FindGameObjectsWithTag("Node");
		player = GameObject.FindGameObjectsWithTag("Player");
		target = player[0].transform.position;
		myBestPath = new List<int>();
	}

	// Update is called once per frame
	void Update () {

		// Check for input from use
		checkInputs();

		// For when player must move along path
		if (followPath == true && count < myBestPath.Count) {
			target = nodesList[findIndexOf(myBestPath[count])].transform.position;
		}

	}


	// Draw lines between all neighbor nodes
	void OnDrawGizmos() {
		if (!showFlipLines) {
			Gizmos.color = Color.black;
			for (int i = 0; i < nodesList.Length; i++) {
				for (int j = 0; j < nodesList[i].GetComponent<NodeController>().neighbors.Length; j++) {
					//Debug.Log ("drawing from " + nodesList[i].GetComponent<NodeController>().myID
					           //+ " to " + nodesList[i].GetComponent<NodeController>().neighbors[j]);
					Gizmos.DrawLine (nodesList[i].transform.position,
					                 nodesList[ findIndexOf(nodesList[i].GetComponent<NodeController>().neighbors[j]) ].transform.position);
				}
			}
		}
	}


	void checkInputs() {

		// While F is pressed, make player agent seek towards mouse position
		if (Input.GetKeyDown(KeyCode.F)) {
			player[0].GetComponent<PlayerAgentController>().seek = !player[0].GetComponent<PlayerAgentController>().seek;
		}

		// When space bar is pressed, show or hide the nodes
		if (Input.GetKeyDown(KeyCode.Space)) {
			for (int i = 0; i < nodesList.Length; i++) {
				nodesList[i].transform.GetComponent<NodeController>().show = showFlip;
			}
			showFlip = !showFlip;
		}

		// When tab is pressed, show or hide the lines between neighboring nodes
		if (Input.GetKeyDown(KeyCode.Tab)) {
			showFlipLines = !showFlipLines;
		}
		
		// When E is pressed, perform A* algorithm
		if (Input.GetKeyDown(KeyCode.E)) {

			//Debug.Log ("executing " + ns + " " + nd);

			// Source is a node
			if (ns != -1 && nd != -1) {
				aStar();
			}

			// Source is the closest node to the player, and player must move
			else if (ns == -1 && nd != -1) {
				ns = findIndexOf(player[0].GetComponent<PlayerAgentController>().findClosestNode());
				if (ns != -1) {
					if (ns != nd) {
						nodesList[ns].GetComponent<NodeController>().isSource = true;
					}
					aStar ();
					player[0].GetComponent<PlayerAgentController>().lockedMovement = true;
					count = 0;
					followPath = true;
				}
				//ns = -1;
			}

		}

		// When R is pressed, reset source and destination
		if (Input.GetKey(KeyCode.R)) {
			if (ns != -1) {
				nodesList[ns].GetComponent<NodeController>().isSource = false;
				ns = -1;
			}
			if (nd != -1) {
				nodesList[nd].GetComponent<NodeController>().isDest = false;
				nd = -1;
			}
			for (int i = 0; i < nodesList.Length; i++) {
				nodesList[i].GetComponent<NodeController>().hasBeenChecked = false;
				nodesList[i].GetComponent<NodeController>().isPath = false;
			}
		}

		// If a node is left-clicked, make it the source
		if (Input.GetMouseButtonDown(0)) {

			if (ns != -1) {
				nodesList[ns].GetComponent<NodeController>().isSource = false;
			}
			Vector3 point = Camera.main.ScreenToWorldPoint( new Vector3(Input.mousePosition.x, Input.mousePosition.y, 
			                                                            -transform.position.z) );
			ns = findClosestNodeToMouse(point);
			if (ns == nd) {
				nd = -1;
			}
			nodesList[ns].GetComponent<NodeController>().isDest = false;
			nodesList[ns].GetComponent<NodeController>().isSource = true;

		}
		
		// If a node is right-clicked, make it the destination
		if (Input.GetMouseButtonDown(1)) {

			if (nd != -1) {
				nodesList[nd].GetComponent<NodeController>().isDest = false;
			}
			Vector3 point = Camera.main.ScreenToWorldPoint( new Vector3(Input.mousePosition.x, Input.mousePosition.y, 
			                                                            -transform.position.z) );
			nd = findClosestNodeToMouse(point);
			if (nd == ns) {
				ns = -1;
			}
			nodesList[nd].GetComponent<NodeController>().isSource = false;
			nodesList[nd].GetComponent<NodeController>().isDest = true;

		}

	} // end of checkInputs


	// Given a mouse position, find the index of the closest node to it
	public int findClosestNodeToMouse(Vector3 p) {

		int mini = 0;
		p.z = 0;
		float minDist = Vector3.Distance(p, nodesList[0].transform.position);

		for (int i = 1; i < nodesList.Length; i++) {
			float tempDist = Vector3.Distance(p, nodesList[i].transform.position);
			if (tempDist < minDist) {
				minDist = tempDist;
				mini = i;
			}
		}

		return mini;
	}


	// Given a node ID, find its index within nodesList
	public int findIndexOf(int id) {
		if (id < 0 || id >= nodesList.Length) {
			return -1;
		}
		for (int i = 0; i < nodesList.Length; i++) {
			if (id == nodesList[i].GetComponent<NodeController>().myID) {
				return i;
			}
		}
		return -1;
	}


	// Perform A* pathfinding search
	public void aStar() {

		string p;
		myQueue = new ArrayList();
		int thisNodeID = nodesList[ns].GetComponent<NodeController>().myID,
		    tempID = nodesList[ns].GetComponent<NodeController>().myID;
		List<int> tempCrumbs = new List<int>();
		tempCrumbs.Add(thisNodeID);
		float startToGoal = Vector3.Distance(nodesList[ns].transform.position, nodesList[nd].transform.position);
		pathNode bestPath = new pathNode(thisNodeID, 0, 0, tempCrumbs),
				 tempPath = new pathNode(thisNodeID, 0, startToGoal, tempCrumbs),
				 tempNeighbor = new pathNode(thisNodeID, 0, 0, tempCrumbs);
		myQueue.Add(tempPath);
		nodesList[ns].GetComponent<NodeController>().hasBeenChecked = true;
		//Debug.Log ("entering while loop");

		// Continue search until destination is reached
		while (findIndexOf(tempID) != nd) {

			//Debug.Log ("checking neighbors of " + thisNodeID + ", crumbs=" + crumbString(tempPath.breadcrumbs));
			// For each neighbor of thisNodeID not yet checked, floodfill from here (add all neighbors to queue)
			for (int i = 0; i < nodesList[findIndexOf(thisNodeID)].GetComponent<NodeController>().neighbors.Length; i++) {

				// Create temporary node object of this neighbor node
				tempID = nodesList[findIndexOf(thisNodeID)].GetComponent<NodeController>().neighbors[i];
				//Debug.Log ("Checking node: " + tempID);
				if (!nodesList[findIndexOf(tempID)].GetComponent<NodeController>().hasBeenChecked) {
					nodesList[findIndexOf(tempID)].GetComponent<NodeController>().hasBeenChecked = true;
					tempCrumbs = copyOf(tempPath.breadcrumbs);
					tempCrumbs.Add (tempID);
					tempNeighbor = new pathNode(tempID,
					                            tempPath.travelled + Vector3.Distance(nodesList[findIndexOf(thisNodeID)].transform.position, 
					                                                                  nodesList[findIndexOf(tempID)].transform.position),
					                            Vector3.Distance(nodesList[findIndexOf(tempID)].transform.position, nodesList[nd].transform.position),
					                            tempCrumbs);

					// Add this neighbor to queue
					myQueue.Add(tempNeighbor);
				}
				else {
					//Debug.Log(tempID + " has been checked");
				}

				// If we reached the goal, set this as best path
				if (findIndexOf(tempID) == nd) {
					bestPath = tempNeighbor;
					myBestPath = bestPath.breadcrumbs;
					break;
				}
			} // checked all neighbors

			if (findIndexOf(tempID) != nd && myQueue.Count > 0) {

				// Sort queue with shorter costs getting priority
				myQueue = sortByCost(myQueue);

				// Set next node to check to be the head of the queue
				tempPath = (pathNode)myQueue[0];
				myQueue.RemoveAt(0);
				thisNodeID = tempPath.id;
				if (!tempPath.breadcrumbs.Contains(thisNodeID)) {
					tempPath.breadcrumbs.Add(thisNodeID);
				}
			}

		} // goal found

		// Loop over A* path and mark intermediate nodes, then wait and unmark them
		p = "best path is: (" + nodesList[ns].GetComponent<NodeController>().myID + ") ";
		if (findIndexOf(bestPath.id) != ns) {
			for (int i = 0; i < bestPath.breadcrumbs.Count; i++) {
				if (findIndexOf(bestPath.breadcrumbs[i]) != ns && findIndexOf(bestPath.breadcrumbs[i]) != nd) {
					nodesList[findIndexOf(bestPath.breadcrumbs[i])].GetComponent<NodeController>().isPath = true;
					p += bestPath.breadcrumbs[i] + " ";
				}
			}
		}
		Debug.Log (p + "(" + nodesList[nd].GetComponent<NodeController>().myID + ")");

	} // end of aStar()


	// Sort the elements in a queue by lowest to highest cost
	public ArrayList sortByCost(ArrayList q) {

		ArrayList ret = new ArrayList();
		pathNode minNode, tempNode;
		float minCost;

		if (q.Count <= 1) {
			return q;
		}
/*
		string b = "Queue before (";
		for (int i = 0; i < q.Count; i++) {
			b += ((pathNode)q[i]).id + ": " + ((pathNode)q[i]).cost + ", ";
		}
		b += ")";
		Debug.Log(b);
*/
		while (q.Count > 0) {
			minNode = (pathNode)q[0];
			q.RemoveAt(0);
			minCost = minNode.cost;
			for (int i = 0; i < q.Count; i++) {
				tempNode = (pathNode)q[0];
				q.RemoveAt(0);
				if (tempNode.cost < minCost) {
					q.Add(minNode);
					minNode = tempNode;
					minCost = tempNode.cost;
				}
				else {
					q.Add(tempNode);
				}
			}
			ret.Add(minNode);
		}
/*
		b = "Queue after (";
		for (int i = 0; i < ret.Count; i++) {
			b += ((pathNode)ret[i]).id + ": " + ((pathNode)ret[i]).cost + ", ";
		}
		b += ")";
		Debug.Log(b);
*/
		return ret;

	} // end of sortByCost()


	// Given a list of ints, return that same list of ints
	public List<int> copyOf(List<int> l) {
		List<int> ret = new List<int>();
		for (int i = 0; i < l.Count; i++) {
			ret.Add(l[i]);
		}
		return ret;
	}


	// Given a pathnode, return a list of breadcrumbs as a string
	public string crumbString(List<int> b) {
		string ret = "";
		for (int i = 0; i < b.Count; i++) {
			ret += b[i] + ",";
		}
		return ret;
	}
}
