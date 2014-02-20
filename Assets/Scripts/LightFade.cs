using UnityEngine;
using System.Collections;
using Holoville.HOTween;

//Fades light on and off smoothly by changing the intensity by a particular rate
public class LightFade : MonoBehaviour {
	public float totalFadeAmount; //The max amount to fade the light
	public float totalDuration; //The total duration of the fade
	public bool startDim; //Tells whether to start out dimming the light or not
	[HideInInspector]
	public bool callStart = true;
	
	private Light lightAffected; //The light to fade
	private float originalIntensity; //The original intensity of the light; may be needed for reference
	private Sequence tweenSequence; //The tween sequence to fade the light in and out
	
	//Use this for initialization
	private void Start() {
		if (callStart == true) {
			lightAffected = gameObject.GetComponent<Light>();
			originalIntensity = lightAffected.intensity;
			
			//Create a new sequence that reverses the order it's played when it's completed and repeats an infinite number of times
			tweenSequence = new Sequence(new SequenceParms().Loops(-1));
			tweenSequence.Append(HOTween.To(lightAffected, (totalDuration / 2), "intensity", getFinalIntensity()));
			tweenSequence.Append(HOTween.To(lightAffected, (totalDuration / 2), "intensity", originalIntensity));
			
			//Play the sequence
			tweenSequence.Play();
		}
	}
	
	public void startSequence(bool take, float intensity, Light LightAffected) {
		lightAffected = LightAffected;
		originalIntensity = lightAffected.intensity;
		
		//Fade to 0 if you're taking the light
		if (take == true) {
			tweenSequence = new Sequence(new SequenceParms().Loops(1).OnComplete(onTakeComplete));
			tweenSequence.Append(HOTween.To(lightAffected, (totalDuration / 2f), "intensity", 0));
		}
		//Otherwise fade to the original intensity
		else {
			lightAffected.enabled = true;
			tweenSequence = new Sequence(new SequenceParms().Loops(1).OnComplete(onPutComplete));
			tweenSequence.Append(HOTween.To(lightAffected, (totalDuration / 2f), "intensity", intensity));
		}
		
		restart();
	}
	
	//Resets the fade - used for light prisoner
	public void reset() {
		this.enabled = false;
		if (tweenSequence != null) {
			tweenSequence.Restart();
			tweenSequence.Pause();
		}
	}
	
	//Restarts the fade - used for light prisoner
	public void restart() {
		this.enabled = true;
		tweenSequence.Play();
	}
	
	//Gets the final intensity the light should go to, being higher or lower depending on whether it started dimming or not
	private float getFinalIntensity() {
		if (startDim == true) return (originalIntensity - totalFadeAmount);
		else return (originalIntensity + totalFadeAmount);
	}
	
	//When the tween is complete (it's finished fading in or out), reverse the tween sequence
	private void onComplete() {
		//tweenSequence.Reverse();
	}
	
	//When the tween is complete after the light prisoner takes the light, end the sequence and disable the light
	private void onTakeComplete() {
		lightAffected.enabled = false;
		Destroy(this);
	}
	
	//When the tween is complete after the light prisoner puts the light, end the sequence
	private void onPutComplete() {
		Destroy(this);
	}
	
	//Update is called once per frame
	private void Update() {
		
	}
}
