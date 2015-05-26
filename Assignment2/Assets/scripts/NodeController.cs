using UnityEngine;
using System.Collections;

public class NodeController : MonoBehaviour {

	public int myID;
	public bool show;
	public bool isSource;
	public bool isDest;
	public bool isPath;
	public bool hasBeenChecked;
	public int[] neighbors;

	// Use this for initialization
	void Start () {
		show = false;
		isSource = false;
		isDest = false;
		isPath = false;
		hasBeenChecked = false;
	}
	
	// Update is called once per frame
	void Update () {

		if (show) {
			renderer.enabled = true;
		}
		else {
			renderer.enabled = false;
		}

		if (isSource) {
			GetComponent<Animator>().SetInteger("state", 1);
		}
		else if (isDest) {
			GetComponent<Animator>().SetInteger("state", 2);
		}
		else if (isPath) {
			GetComponent<Animator>().SetInteger("state", 3);
		}
		else {
			GetComponent<Animator>().SetInteger("state", 0);
		}

	}
}
