using UnityEngine;
using System.Collections;
using Holoville.HOTween;

//Turns a light on or off
public class ToggleLight : MonoBehaviour {
	public bool fade; //An option to have the light fade in or out instead of suddenly turn on or off
	public float fadeDuration; //The duration of the fade in or out
	
	private Light lightAffected; //The light affected by the script
	private float originalIntensity; //The original intensity of the light
	private Tweener tween; //The tween to fade the light in and out
	
	//Use this for initialization
	private void Start() {
		lightAffected = gameObject.GetComponent<Light>();
		originalIntensity = lightAffected.intensity;
	}
	
	//Resets the light back to its original intensity - used for the Light prisoner
	public void resetIntensity() {
		lightAffected.intensity = originalIntensity;
	}
	
	//Plays a fade when the player enters or exits the light trigger
	private void playFade(bool enterLight) {
		//if (tween != null && tween.isComplete == false) tween.Complete();
		tween = HOTween.To(lightAffected, fadeDuration, "intensity", enterLight == true ? 0 : originalIntensity);
		tween.autoKillOnComplete = false;
		tween.Play();
	}
	
	//Turn the light on or off if the player gets near
	private void OnTriggerEnter(Collider other) {
		if (fade == false)
			lightAffected.enabled = !lightAffected.enabled;
		else playFade(true);
	}
	
	//Turn the light on or off if the player exits the light
	private void OnTriggerExit(Collider other) {
		if (fade == false)
			lightAffected.enabled = !lightAffected.enabled;
		else playFade(false);
	}
	
	//Update is called once per frame
	private void Update() {
	
	}
}
