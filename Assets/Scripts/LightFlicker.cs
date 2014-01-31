using UnityEngine;
using System.Collections;

//Makes a light flicker based on a timer. The light can be on or off initially; set in the inspector by enabling/disabling it
//Attach this script to an object with light. 
//The light will stay in its initial state for a duration, then flicker to the opposite state (ex. on if off) for a duration a couple of times, afterwards returning to its initial state
//There is also an option to simply dim the lights instead of turn them off
public class LightFlicker : MonoBehaviour {
	public float minInitialDuration; //The min length of time the light is in the initial state, in seconds
	public float maxInitialDuration; //The max length of time the light can be in the initial state, in seconds
	public float minFlickerDuration; //The min length of time the light is in the opposite state, in seconds
	public float maxFlickerDuration; //The max length of time the light is in the opposite state, in seconds
	public int minFlicker; //The minimum amount of times the light flickers
	public int maxFlicker; //The maximum amount of time the light flickers
	public float dimAmount; //The amount to dim the light by if dim is on
	public bool dimLight; //Tells if the light should be dimmed or not
	
	private Light lightAffected; //The light reference this is affecting
	private float originalIntensity; //The original light intensity
	private float nextFlicker; //The next time a light flickers
	private int flicker; //Tells what stage of flicker the light is supposed to flicker next or not. 0 is the beginning
	
	//Use this for initialization
	private void Start() {
		lightAffected = gameObject.GetComponent<Light>();
		originalIntensity = lightAffected.intensity;
		
		nextFlicker = Time.time + randomInitialDuration();
		flicker = 0;
	}
	
	//Choose a random initial duration in the min and max range
	private float randomInitialDuration() {
		return (Random.Range(minInitialDuration, maxInitialDuration));
	}
	
	//Choose a random flicker duration in the min and max range
	private float randomFlickerDuration() {
		return (Random.Range(minFlickerDuration, maxFlickerDuration));
	}
	
	//Checks the value to add to the intensity of the next flicker if dim is selected (+ for flicker = true, - for flicker = false)
	private float getDimValue() {
		//If the light previously dimmed, brighten it
		if (lightAffected.intensity < originalIntensity) return dimAmount;
		else return -dimAmount;
	}
	
	//Checks the value of the number of flicker times (flicker = 0, initialDuration, otherwise flickerDuration)
	private float getNextFlicker() {
		if (flicker == 0) return (Time.time + randomInitialDuration());
		else return (Time.time + randomFlickerDuration());
	}
	
	//Update is called once per frame
	private void Update() {
		//Turn the light on and off based on a timer
		if (Time.time >= nextFlicker) {
			//Switch the light on/off if dim isn't selected
			if (dimLight == false) lightAffected.enabled = !lightAffected.enabled;
			else lightAffected.intensity += getDimValue();
			
			//Add 1 to the flicker counter, then mod it with the number of flickers that range from the min to the max
			flicker = (flicker + 1) % Random.Range(minFlicker, maxFlicker);
			
			//Depending on if the light should flicker next or not, set the next on/off time
			nextFlicker = getNextFlicker();
		}
	}
}