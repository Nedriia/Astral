using UnityEngine;
using System.Collections;

public class Prisoner : MonoBehaviour {
	
	protected int health;
	protected float speed;
	protected float jumpVelocity;
	protected bool isPossessed;
	protected GameObject selectedItem;
	//inventory
	
	// Use this for initialization
	private void Start () {
		health = 100;
		speed = 5.0f;
		jumpVelocity = 10.0f;
		isPossessed = false;
	}
	
	// Update is called once per frame
	private void Update () {
		if (Input.GetKeyDown(KeyCode.R))
		{
			//deleteFromInventory ();
		}
		else if (Input.GetKeyDown(KeyCode.F))
		{
			//useItem();
		}
		else if (Input.GetKeyDown(KeyCode.V))
		{
			//browseItem();
		}
		else if (Input.GetKeyDown(KeyCode.G))
		{
			//activateSwitch();
		}
		
	}

	/*
	private void onTriggerEnter(Collider other) {
		Debug.Log("anything?");
		if (Input.GetKeyDown(KeyCode.E) && other.tag == "Item") {
			Debug.Log("done");
			other.gameObject.SetActive(false);
			//addToInventory(other.gameObject);
		}
	}*/
	
	
	/*Empty functions
	private void addToInventory(GameObject item) {
	}
	private void deleteFromInventory(GameObject item) {
	}
	private void deleteFromInventory(int item) {
	}
	private void useItem() {
	}
	private void browseItem() {
	}
	private void activateSwitch() {
	}
	*/
}
