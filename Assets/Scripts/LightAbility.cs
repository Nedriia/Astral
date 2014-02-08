using UnityEngine;
using System.Collections;

//The light prisoner
public class LightAbility : MonoBehaviour {
	private Light lightStored; //The light the prisoner has stored. The prisoner can start with light

	//Use this for initialization
	private void Start() {
		
	}
	
	//NOTE: Probably will be switched to raycasting later; this is just a quick test to get the algorithm down
	private void OnTriggerEnter(Collider other) {
		//Get the light of the object
		AbsorbableLight light = other.gameObject.GetComponent<AbsorbableLight>();
		
		//If there is indeed a light and it is enabled, make it flicker brighter to indicate that the light prisoner can take it
		if (light != null) {
			light.indicateTake();
		}
	}
	
	//Take the light from the light source and store it
	private void OnTriggerStay(Collider other) {
		//Get the light of the object
		AbsorbableLight light = other.gameObject.GetComponent<AbsorbableLight>();
		
		//If there is a light, and the Player pressed the L button, store the light if the light is enabled and put the light back if the light is disabled
		if (light != null) {
			//If you put the light back and are still standing on it, check for enabling it again
			if (light.getSpecialFlicker == null) {
				light.indicateTake();
			}
			
			if (Input.GetKeyDown(KeyCode.L) == true) {
				if (light.canBeTaken() == true) {
					lightStored = light.takeLight();
				}
				else if (light.canBePlaced() == true) {
					light.placeLight(lightStored);
					
					//Release the light
					lightStored = null;
				}
			}
		}
	}
	
	//Disable the light when the prisoner walks out of it
	private void OnTriggerExit(Collider other) {
		//Get the light of the object
		AbsorbableLight light = other.gameObject.GetComponent<AbsorbableLight>();
		
		//If there is indeed a light and it is enabled, disable its flicker
		if (light != null) {
			light.stopTake();
		}
	}
	
	private bool isInLight() {
		return false;
	}
	
	//Update is called once per frame
	private void Update() {
	
	}
}
