using UnityEngine;
using System.Collections;
using Holoville.HOTween;

//Placed on all lights that the Light Prisoner can absorb
public class AbsorbableLight : MonoBehaviour {
	private Light lightAffected; //The light absorbed or put back into
	private float originalIntensity; //The original intensity of the light
	private bool hasLight; //Checks if the light is actually in the source or not
	private Wire wire; //The wire for the light
	
	private LightFlicker flicker; //The light's special flicker that indicates when the prisoner can put light
	private LightFade fade; //The light's special fade that gets added when the prisoner takes the light
	private Tweener dimmer; //The light's special dim that indicates that the prisoner can take light
	
	//We need these properties to ensure that all lights that are on behave the same way, regardless of what they have attached
	private LightFlicker originalFlicker; //The light's original flicker
	private LightFade originalFade; //The light's original fade
	private ToggleLight toggle; //The light's toggle
	
	//Constants for the shaders
	private Shader glowShader;
	private Shader noGlowShader;
	
	//Use this for initialization
	private void Start() {
		//Get the light
		lightAffected = this.gameObject.GetComponent<Light>();
		
		//If the light is found, get the original intensity and whether the light has 
		if (lightAffected != null) {
			originalIntensity = lightAffected.intensity;
			hasLight = lightAffected.enabled;
		}
		
		//Get the Light's wire
		wire = this.gameObject.GetComponentInChildren<Wire>();
		
		//Set the special effects that occur when the light prisoner interacts with the light
		flicker = null;
		fade = null;
		dimmer = null;
		
		//Find the original effects that the light has
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
	
	//Tells if the light has a light actually inside it
	public bool containsLight {
		get { return hasLight; }
	}
	
	//Gets the special flicker
	public LightFlicker getSpecialFlicker {
		get { return flicker; }
	}
	
	//Gets the special dim
	public Tweener getSpecialDim {
		get { return dimmer; }
	}
	
	//Gets the light's wire
	public Wire getWire {
		get { return wire; }
	}
	
	//Makes the light dim when light prisoner goes under it without light stored
	public void indicateTake(LightPrisoner lightPrisoner) {
		if (canBeTaken() == true) {
			//Disable the other lights, set this light back to its original intensity, and make it flicker
			modifyLightTypes(false);
			
			lightAffected.intensity = originalIntensity;
		
			dimmer = HOTween.To(lightAffected, lightPrisoner.specialdim.dimTime, "intensity", originalIntensity - lightPrisoner.specialdim.dimAmount);
			dimmer.autoKillOnComplete = true;
			if (wire != null) wire.changeWireShader(lightAffected, true);
		}
	}
	
	//Makes the light brighten when the light prisoner goes under it with light stored
	public void indicatePut(LightPrisoner lightPrisoner) {
		if (canIndicatePlace() == true && flicker == null) {
			//Disable the other lights, set this light back to its original intensity, and make it flicker
			modifyLightTypes(false);
			
			if (dimmer != null) dimmer.Complete();
			lightAffected.enabled = true;
			lightAffected.intensity = 0f;
			
			dimmer = HOTween.To(lightAffected, lightPrisoner.specialdim.brightenTime, "intensity", originalIntensity + lightPrisoner.specialdim.brightenAmount);
			dimmer.autoKillOnComplete = true;
			//lightAffected.intensity = originalIntensity;
		
			//Add a flicker
			//flicker = this.gameObject.AddComponent("LightFlicker") as LightFlicker;
			//lightPrisoner.specialflicker.setFlicker(flicker);
			if (wire != null) wire.changeWireShader(lightAffected, true);
		}
	}
	
	//Reverts the light back to its original state when the light prisoner leaves it
	public void stopTake(LightPrisoner lightPrisoner) {
		//Turn the original properties back on and reset the light's intensity to its original value
		modifyLightTypes(true);
		if (dimmer != null) dimmer.Complete();
		dimmer = HOTween.To(lightAffected, lightPrisoner.specialdim.dimTime, "intensity", originalIntensity);
		dimmer.autoKillOnComplete = true;
		if (wire != null) wire.changeWireShader(lightAffected, false);
	}
	
	public void stopPut(LightPrisoner lightPrisoner) {
		if (hasLight == false) {
			//Turn the original properties back on and reset the light's intensity to its original value
			modifyLightTypes(true);
			lightAffected.enabled = false;
			if (dimmer != null) dimmer.Complete();
			dimmer = HOTween.To(lightAffected, lightPrisoner.specialdim.brightenTime, "intensity", originalIntensity);
			dimmer.autoKillOnComplete = true;
			//Destroy(flicker);
			if (wire != null) wire.changeWireShader(lightAffected, false);
		}
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
		return (lightAffected != null && hasLight == true && fade == null);
	}
	
	//Checks if the light can be placed
	public bool canBePlaced() {
		return (lightAffected != null && hasLight == false && fade == null);
	}
	
	//Checks if the light can be indicated that it's taken
	public bool canIndicatePlace() {
		return (lightAffected != null && hasLight == false && lightAffected.enabled == false && fade == null);
	}
	
	//Gives light to the light prisoner
	public AbsorbableLight takeLight(LightPrisoner lightPrisoner) {
		//Bring the light back to normal intensity
		lightAffected.intensity = originalIntensity;
		hasLight = false;
		
		//End the dim and add a temp fade
		dimmer.Kill();
		dimmer = null;
		addTempFade(true, lightPrisoner);
		
		//Return this component to get the light values and original intensity
		return this;
	}
	
	//Gives light from the light prisoner to this light
	public void placeLight(Light light, LightPrisoner lightPrisoner) {
		//lightAffected = light;
		hasLight = true;
		
		//Destroy(flicker);
		if (dimmer != null) dimmer.Complete();
		
		//Reset the intensity of the light and change its color to the new light's color
		lightAffected.intensity = 0f;
		lightAffected.color = light.color;
		
		//Add a temp fade
		addTempFade(false, lightPrisoner);
	}
	
	private void addTempFade(bool take, LightPrisoner lightPrisoner) {
		fade = this.gameObject.AddComponent("LightFade") as LightFade;
		lightPrisoner.specialfade.setFade(take, fade, originalIntensity, lightAffected);
	}
	
	//Update is called once per frame
	private void Update() {
		
	}
}