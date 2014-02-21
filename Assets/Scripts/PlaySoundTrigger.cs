using UnityEngine;
using System;
using System.Collections;

public class PlaySoundTrigger : MonoBehaviour {
	public string objectName; //The name of the game object that is required to pass through the trigger to play the sound
	
	private AudioSource SFX; //The sound effect to play
	private bool played; //Tells if the sound effect already played or not
	
	// Use this for initialization
	private void Start() {
		SFX = gameObject.GetComponent<AudioSource>();
		played = false;
	}
	
	//Play the sound when you enter the trigger if the sound isn't null and it hasn't been played already
	//If an object name is specified (not empty or null), play the sound only when an object with that name passes through the trigger
	private void OnTriggerEnter(Collider other) {
		if (SFX != null && played == false && (String.IsNullOrEmpty(objectName) == true || (objectName == other.gameObject.name))) {
			SFX.Play();
			played = true;
		}
	}
	
	//Update is called once per frame
	private void Update() {
	
	}
}
