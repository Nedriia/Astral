using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//A switch that manipulates an object by rotating, translating, or scaling it
public class Switch : MonoBehaviour {
	public Transform objectManipulated; //The object to manipulate
	private List<string> switchAnimNames;
	private List<string> objectAnimNames;
	
	private Animation switchAnim;
	private Animation objectAnim;
	
	private bool activated; //Indicates if the switch is activated or not
	
	//Use this for initialization
	private void Start() {
		activated = false;
		
		//switchAnimNames = new List<string>();
		
		//Find an animation component and get the names of the animations in the component. We need only 2 animations; one for activating the switch, and one for deactivating the switch
		objectAnim = objectManipulated.GetComponent<Animation>();
		
		//Add all the animations that the animation component has (for some reason Unity doesn't let you easily access them...)
		if (objectAnim != null) {
			objectAnimNames = new List<string>();
			foreach (AnimationState anim in objectAnim)
				objectAnimNames.Add(anim.name);
		}
	}
	
	//Check if a prisoner pulled the switch
	private void OnTriggerStay(Collider other) {
		//Check if it was a prisoner, the object isn't moving from the switch, and the player pressed the F key
		if (objectManipulated != null && other.gameObject.tag == "Prisoner" && objectAnim.isPlaying == false && Input.GetKeyDown(KeyCode.F) == true) {
			//If the switch isn't activated, play the animation when it gets activated
			if (activated == false) {
				objectAnim.Play(objectAnimNames[0]);
			}
			//Otherwise, play the animation when it gets deactivated
			else {
				objectAnim.Play(objectAnimNames[1]);
			}
			
			activated = !activated;
		}
	}
	
	//Update is called once per frame
	private void Update() {
	
	}
}
