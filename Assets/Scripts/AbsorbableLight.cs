using UnityEngine;
using System.Collections;

//Placed on all lights that the light prisoner can absorb. It must have the same effect regardless of what the light has attached to it
//So, what we need to do is have each light flicker in the SAME way when the light prisoner is under, and we must disable all the other properties when this happens
//If everything works out fine, we should just have to include this in a light and it'll work
public class AbsorbableLight : MonoBehaviour {
	private Light lightAffected; //The light absorbed or put back into
	private float originalIntensity; //The original intensity of the light
	
	private LightFlicker flicker; //The light's special flicker that gets added when the prisoner is under the light
	private LightFade fade; //The light's special fade that gets added when the prisoner takes the light
	
	//We need these properties to ensure that all lights that are on behave the same way, regardless of what they have attached
	private LightFlicker originalFlicker; //The light's original flicker
	private LightFade originalFade; //The light's original fade
	private ToggleLight toggle; //The light's toggle
	
	//Use this for initialization
	private void Start() {
		lightAffected = this.gameObject.GetComponent<Light>();
		if (lightAffected != null) originalIntensity = lightAffected.intensity;
		
		flicker = null;
		fade = null;
		
		originalFlicker = this.gameObject.GetComponent<LightFlicker>();
		originalFade = this.gameObject.GetComponent<LightFade>();
		toggle = this.gameObject.GetComponent<ToggleLight>();
	}
	
	//Gets the light affected
	public Light getLight {
		get { return lightAffected; }
	}
	
	//Gets the original intensity of the light
	public float getOrigIntensity {
		get { return originalIntensity; }
	}
	
	//Gets the special flicker
	public LightFlicker getSpecialFlicker {
		get { return flicker; }
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
		
		//Turn the original properties back on and reset the light's intensity to its original value
		modifyLightTypes(true);
		lightAffected.intensity = originalIntensity;
	}
	
	//Enables or disables the types of light
	private void modifyLightTypes(bool enabled) {
		//Enable or disable the original flickers or fades to not interfere with the actions the light prisoner does
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
	public AbsorbableLight takeLight() {
		//Bring the light back to normal intensity
		lightAffected.intensity = originalIntensity;
		
		//Destroy the temp flicker and add a temp fade
		Destroy(flicker);
		addTempFade(true);
		
		//Return this component to get the light values and original intensity
		return this;
	}
	
	//Gives light from the light prisoner to this light
	public void placeLight(Light light) {
		//lightAffected = light;
		
		//Reset the intensity of the light and change its color to the new light's color
		lightAffected.intensity = 0f;
		lightAffected.color = light.color;
		
		//Add a temp fade
		addTempFade(false);
	}
	
	private void addTempFade(bool take) {
		fade = this.gameObject.AddComponent("LightFade") as LightFade;
		fade.specialFade(take, lightAffected, originalIntensity);
	}
	
	//Update is called once per frame
	private void Update() {
	
	}
}
