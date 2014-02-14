using UnityEngine;
using System.Collections;

public class Prisoner : MonoBehaviour, Possessable {
    
    public FreeLookCam prisonerCamera;

	protected int health;
	protected float speed;
	protected float jumpVelocity;
	protected bool isPossessed;
	protected GameObject selectedItem;

    private Animation eyes;
    private Animator prisonerAnimator;
    private ThirdPersonUserControl userControl;
	//inventory
	
	// Use this for initialization
	private void Start () {
		health = 100;
		speed = 5.0f;
		jumpVelocity = 10.0f;
		isPossessed = false;
        eyes = prisonerCamera.gameObject.GetComponent<Animation>();
        userControl = gameObject.GetComponent<ThirdPersonUserControl>();
        prisonerAnimator = gameObject.GetComponent<Animator>();

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
            waitTime = eyes["Prisoner Eyes Open"].length;
        } else {
            eyes.Play("Prisoner Eyes Close");
            waitTime = eyes["Prisoner Eyes Close"].length;
        }
        return waitTime;
    }


    public void startControlling() {
        userControl.enabled = true;
    }

    public void stopControlling() {
        userControl.enabled = false;
        prisonerAnimator.SetFloat("Forward", 0);
        prisonerAnimator.SetFloat("Turn", 0);
    }
	
	//Kills the prisoner
	public void Die() {
		//Get prisoner components
		Rigidbody prisonerbody = this.gameObject.GetComponent<Rigidbody>();
		Animator prisoneranim = this.gameObject.GetComponent<Animator>();
		ThirdPersonCharacter thirdperson = this.gameObject.GetComponent<ThirdPersonCharacter>();
		
		//Modify the rigidbody to act like a ragdoll
		prisonerbody.constraints = RigidbodyConstraints.None;
		prisonerbody.angularDrag = 1f;
		prisonerbody.useGravity = true;
		prisonerbody.isKinematic = false;
		prisonerbody.AddTorque(new Vector3(150, 0, 0));
		
		//Disable the prisoner animator and the thirdpersoncharacter and prisoner scripts
		prisoneranim.enabled = false;
		thirdperson.enabled = false;
		this.enabled = false;
		
		//Remove the prisoner from the prisoner inventory
		GameObject.Find("Possession Master").GetComponent<PossessionMaster>().getInventory().Remove(this);
		
		/*foreach (Transform child in prisoner.transform) {
			foreach (Transform childc in child.transform) {
				if (childc.renderer != null)
					childc.renderer.material.color = new Color(139, 69, 19);
			}
		}*/
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
