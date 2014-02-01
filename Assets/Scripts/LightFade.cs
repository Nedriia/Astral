using UnityEngine;
using System.Collections;
using Holoville.HOTween;

//Fades light on and off smoothly by changing the intensity by a particular rate
public class LightFade : MonoBehaviour {
	public float totalFadeAmount; //The max amount to fade the light
	public float totalDuration; //The total duration of the fade
	public bool startDim; //Tells whether to start out dimming the light or not
	
	private Light lightAffected; //The light to fade
	private float originalIntensity; //The original intensity of the light; may be needed for reference
	private Sequence tweenSequence; //The tween sequence to fade the light in and out

	//Use this for initialization
	private void Start() {
		lightAffected = gameObject.GetComponent<Light>();
		originalIntensity = lightAffected.intensity;
		
		//Create a new sequence that reverses the order it's played when it's completed and repeats an infinite number of times
		tweenSequence = new Sequence(new SequenceParms().Loops(-1).OnComplete(onComplete));
		tweenSequence.Append(HOTween.To(lightAffected, (totalDuration / 2), "intensity", getFinalIntensity()));
		tweenSequence.Append(HOTween.To(lightAffected, (totalDuration / 2), "intensity", originalIntensity));
		
		//Play the sequence
		tweenSequence.Play();
	}
	
	//Gets the final intensity the light should go to, being higher or lower depending on whether it started dimming or not
	private float getFinalIntensity() {
		if (startDim == true) return (originalIntensity - totalFadeAmount);
		else return (originalIntensity + totalFadeAmount);
	}
	
	//When the tween is complete (it's finished fading in or out), reverse the tween sequence
	private void onComplete() {
		tweenSequence.Reverse();
	}
	
	//Update is called once per frame
	private void Update() {
		
	}
}
