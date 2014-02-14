﻿using UnityEngine;
using System;
using System.Collections;

//The light prisoner
public class LightAbility : MonoBehaviour {
	public specialFlicker specialflicker; //A special flicker
	public specialFade specialfade; //A special fade

	private GameObject lightHolder; //The game object holding the light that the prisoner has attached
	private Light lightStored; //The light the prisoner has stored. The prisoner can start with light
	private LightFade lightStoredFade; //The prisoner's light fade
	
	//Use this for initialization
	private void Start() {
		lightHolder = GameObject.Find("Light Prisoner/StoredLight");
		lightStored = lightHolder.GetComponent<Light>();
		lightStoredFade = null;
	}
	
	//NOTE: Probably will be switched to raycasting later; this is just a quick test to get the algorithm down
	private void OnTriggerEnter(Collider other) {
		//Get the light of the object
		AbsorbableLight light = other.gameObject.GetComponent<AbsorbableLight>();
		
		//If there is indeed a light and the light prisoner doesn't have one, make it flicker brighter to indicate that the light prisoner can take it
		if (lightStored.enabled == false && light != null) {
			light.indicateTake(this);
		}
	}
	
	//Take the light from the light source and store it
	private void OnTriggerStay(Collider other) {
		//Get the light of the object
		AbsorbableLight light = other.gameObject.GetComponent<AbsorbableLight>();
		
		//If there is a light, and the Player pressed the L button, store the light if the light is enabled and put the light back if the light is disabled
		if (light != null) {
			//If you put the light back and are still standing on it, check for enabling it again
			if (lightStored.enabled == false && light.getSpecialFlicker == null) {
				light.indicateTake(this);
			}
			
			//Check if the player presses L
			if (Input.GetKeyDown(KeyCode.L) == true) {
				//Take the light if it can be taken
				if (lightStored.enabled == false && light.canBeTaken() == true) {
					//Get the light properties
					AbsorbableLight lighttaken = light.takeLight(this);
					
					lightStored.color = lighttaken.getLight.color;
					
					addTempFade(false, lighttaken.getOrigIntensity);
				}
				//Put your light into a light source if you have light and the light source can receive the light
				else if (lightStored.enabled == true && lightStoredFade == null && light.canBePlaced() == true) {
					light.placeLight(lightStored, this);
					
					addTempFade(true, 0f);
					//Release the light
					//lightStored = null;
				}
			}
		}
	}
	
	//Disable the light when the prisoner walks out of it
	private void OnTriggerExit(Collider other) {
		//Get the light of the object
		AbsorbableLight light = other.gameObject.GetComponent<AbsorbableLight>();
		
		//If there is indeed a light and it is enabled, disable its flicker
		if (light != null && light.getSpecialFlicker != null) {
			light.stopTake();
		}
	}
	
	private bool isInLight() {
		return false;
	}
	
	//Sets the temp fade, showing the transfer of light from the light source to the light prisoner or vice versa
	private void addTempFade(bool put, float intensity) {
		lightStoredFade = lightStored.gameObject.AddComponent("LightFade") as LightFade;
		specialfade.setFade(put, lightStoredFade, intensity, lightStored);
		//lightStoredFade.specialFade(put, lightStored, intensity);
	}
	
	//Update is called once per frame
	private void Update() {
	
	}
	
	[Serializable]
	public class specialFlicker {
		public float minInitDur;
		public float maxInitDur;
		public float minFlickDur;
		public float maxFlickDur;
		public int minFlickNum;
		public int maxFlickNum;
		public float dimAmount;
		public bool dimLight;
		
		public void setFlicker(LightFlicker flicker) {
			flicker.minInitialDuration = minInitDur;
			flicker.maxInitialDuration = maxInitDur;
			flicker.minFlickerDuration = minFlickDur;
			flicker.maxFlickerDuration = maxFlickDur;
			flicker.minFlicker = minFlickNum;
			flicker.maxFlicker = maxFlickNum;
			flicker.dimAmount = dimAmount;
			flicker.dimLight = dimLight;
		}
	}
	
	[Serializable]
	public class specialFade {
		public float fadeDuration;
		
		public void setFade(bool take, LightFade fade, float intensity, Light lightAffected) {
			fade.totalDuration = fadeDuration;
			fade.startDim = true;
			fade.callStart = false;
			
			fade.startSequence(take, intensity, lightAffected);
		}
	}
}
