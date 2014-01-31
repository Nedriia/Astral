using UnityEngine;
using System.Collections;

public class Prisoner : MonoBehaviour, Possessable {
	
	protected int health;
	protected float speed;
	protected float jumpVelocity;
	protected bool isPossessed;
	protected GameObject selectedItem;

    private Animation eyes;
    private bool eyesOpen;
    private ThirdPersonUserControl userControl;
	//inventory
	
	// Use this for initialization
	private void Start () {
        eyesOpen = false;
		health = 100;
		speed = 5.0f;
		jumpVelocity = 10.0f;
		isPossessed = false;
        eyes = gameObject.GetComponent<Animation>();
        userControl = gameObject.GetComponent<ThirdPersonUserControl>();
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

    //true for in, false for out, return amount of time to wait
    public float bodyTransition(bool entering) {
        float waitTime = 0;
        if (entering) {
            eyes.Play();
            eyesOpen = false;
            waitTime = eyes["Eyes Open"].length;
        } else {
            eyes.Play("Eyes Close");
            eyesOpen = true;
            waitTime = eyes["Eyes Close"].length;
        }
        return waitTime;
    }


    public void startControlling() {
        userControl.enabled = true;
    }

    public void stopControlling() {
        userControl.enabled = false;
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
