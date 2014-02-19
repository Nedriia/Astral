using UnityEngine;
using System.Collections;
using Holoville.HOTween;

//Placed on all lights that the light prisoner can absorb. It must have the same effect regardless of what the light has attached to it
//So, what we need to do is have each light flicker in the SAME way when the light prisoner is under, and we must disable all the other properties when this happens
//If everything works out fine, we should just have to include this in a light and it'll work
public class AbsorbableLight : MonoBehaviour {
	private Light lightAffected; //The light absorbed or put back into
	private float originalIntensity; //The original intensity of the light
	private bool hasLight; //Checks if the light is actually in the source or not
	
	private LightFlicker flicker; //The light's special flicker that indicates when the prisoner can put light
	private LightFade fade; //The light's special fade that gets added when the prisoner takes the light
	private Tweener dimmer; //The light's special dim that indicates that the prisoner can take light
	
	//We need these properties to ensure that all lights that are on behave the same way, regardless of what they have attached
	private LightFlicker originalFlicker; //The light's original flicker
	private LightFade originalFade; //The light's original fade
	private ToggleLight toggle; //The light's toggle
	private Renderer wireRenderer; //The mesh renderer of the wire, which has the material of the wire
	
	//Use this for initialization
	private void Start() {
		lightAffected = this.gameObject.GetComponent<Light>();
		if (lightAffected != null) 
		{
			originalIntensity = lightAffected.intensity;
			hasLight = lightAffected.enabled;
		}
		
		flicker = null;
		fade = null;
		dimmer = null;
		
		originalFlicker = this.gameObject.GetComponent<LightFlicker>();
		originalFade = this.gameObject.GetComponent<LightFade>();
		toggle = this.gameObject.GetComponent<ToggleLight>();
		wireRenderer = this.gameObject.GetComponentInChildren<Renderer>();
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
	
	//Gets the special dim
	public Tweener getSpecialDim {
		get { return dimmer; }
	}
	
	//Makes the light dim when light prisoner goes under it without light stored
	public void indicateTake(LightPrisoner lightPrisoner) {
		if (canBeTaken() == true) {
			//Disable the other lights, set this light back to its original intensity, and make it flicker
			modifyLightTypes(false);
			
			lightAffected.intensity = originalIntensity;
		
			dimmer = HOTween.To(lightAffected, lightPrisoner.specialdim.dimTime, "intensity", originalIntensity - lightPrisoner.specialdim.dimAmount);
			dimmer.autoKillOnComplete = true;
			if (wireRenderer != null) wireRenderer.material.color = new Color(wireRenderer.material.color.r, wireRenderer.material.color.g, wireRenderer.material.color.b, .2f);
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
			if (wireRenderer != null) wireRenderer.material.color = new Color(wireRenderer.material.color.r, wireRenderer.material.color.g, wireRenderer.material.color.b, .2f);
		}
	}
	
	//Reverts the light back to its original state when the light prisoner leaves it
	public void stopTake(LightPrisoner lightPrisoner) {
		//Turn the original properties back on and reset the light's intensity to its original value
		modifyLightTypes(true);
		if (dimmer != null) dimmer.Complete();
		dimmer = HOTween.To(lightAffected, lightPrisoner.specialdim.dimTime, "intensity", originalIntensity);
		dimmer.autoKillOnComplete = true;
		if (wireRenderer != null) wireRenderer.material.color = new Color(wireRenderer.material.color.r, wireRenderer.material.color.g, wireRenderer.material.color.b, 0f);
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
			if (wireRenderer != null) wireRenderer.material.color = new Color(wireRenderer.material.color.r, wireRenderer.material.color.g, wireRenderer.material.color.b, 0f);
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
