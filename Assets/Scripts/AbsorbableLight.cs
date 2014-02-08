using UnityEngine;
using System.Collections;
using Holoville.HOTween;

//Placed on all lights that the light prisoner can absorb. It must have the same effect regardless of what the light has attached to it
//So, what we need to do is have each light flicker in the SAME way when the light prisoner is under, and we must disable all the other properties when this happens
//If everything works out fine, we should just have to include this in a light and it'll work
public class AbsorbableLight : MonoBehaviour {
	private Light lightAffected; //The light absorbed or put back into
	private float originalIntensity; //The original intensity of the light
	
	//We need these properties to ensure that all lights that are on behave the same way, regardless of what they have attached
	private LightFlicker originalFlicker; //The light's original flicker
	private LightFlicker flicker; //The light's special flicker that gets added when the prisoner is under the light
	private LightFade originalFade; //The light's original fade
	private LightFade fade; //The light's special fade that gets added when the prisoner takes the light
	private ToggleLight toggle; //The light's toggle
	
	//Use this for initialization
	private void Start() {
		lightAffected = this.gameObject.GetComponent<Light>();
		if (lightAffected != null) originalIntensity = lightAffected.intensity;
		
		originalFlicker = this.gameObject.GetComponent<LightFlicker>();
		flicker = null;
		originalFade = this.gameObject.GetComponent<LightFade>();
		fade = null;
		toggle = this.gameObject.GetComponent<ToggleLight>();
		
		//Create a new sequence that reverses the order it's played when it's completed and repeats an infinite number of times
		//tweenSequence = new Sequence(new SequenceParms().Loops(-1).OnComplete(onComplete));
		//tweenSequence.Append(HOTween.To(lightAffected, (totalDuration / 2), "intensity", getFinalIntensity()));
	}
	
	//Gets the light affected
	public Light getLight {
		get { return lightAffected; }
	}
	
	//Gets the special flicker
	public LightFlicker getSpecialFlicker {
		get { return flicker; }
	}
	
	public void onComplete() {
		//tweenSequence.Stop();
		//tweenSequence.
	}
	
	//Makes the light start flickering when the light prisoner goes under it. This flicker is special and is the same for each light that can be taken
	public void indicateTake() {
		if (canBeTaken() == true) {
			//Disable the other lights, set this light back to its original intensity, and make it flicker
			modifyLightTypes(false);
			
			lightAffected.intensity = originalIntensity;
			
			//Add a flicker
			flicker = this.gameObject.AddComponent("LightFlicker") as LightFlicker;
			flicker.specialFlicker();
		}
	}
	
	//Reverts the light back to its original state when the light prisoner leaves it
	public void stopTake() {
		Destroy(flicker);
		modifyLightTypes(true);
		
		lightAffected.intensity = originalIntensity;
	}
	
	//Enables or disables the types of light
	private void modifyLightTypes(bool enabled) {
		if (originalFlicker != null) originalFlicker.enabled = enabled;
		if (originalFade != null) {
			if (enabled == true) originalFade.restart();
			else originalFade.reset();
		}
		if (toggle != null) toggle.enabled = enabled;
	}
	
	//Checks if the light can be taken
	public bool canBeTaken() {
		return (lightAffected != null && lightAffected.enabled == true && fade == null);
	}
	
	//Checks if the light can be placed
	public bool canBePlaced() {
		return (lightAffected != null && lightAffected.enabled == false && fade == null);
	}
	
	//Gives light to the light prisoner
	public Light takeLight() {
		lightAffected.intensity = originalIntensity;
		
		Destroy(flicker);
		fade = this.gameObject.AddComponent("LightFade") as LightFade;
		fade.specialFade(true, lightAffected, originalIntensity);
		
		return lightAffected;
	}
	
	//Gives light from the light prisoner to this light
	public void placeLight(Light light) {
		//lightAffected = light;
		
		fade = this.gameObject.AddComponent("LightFade") as LightFade;
		fade.specialFade(false, lightAffected, originalIntensity);
	}
	
	//Update is called once per frame
	private void Update() {
	
	}
}
