using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pearl : Prisoner
{
	public float MAXFEAR = 5.0f;
	public float MAXTIMESCREAM = 2.0f;
	public float SCARERATE = 1.0f;
	public float SCAREDECRATE = 3.0f;
	public float scaredPercentage = 0.4f;
	public float lightDec1 = 0.3f;
	public float lightDec2 = 0.5f;
	
	private enum state
	{
		NotScared,
		Scared,
		Terrified,
		Paralyzed}
	;
	private state scaredState;
	private GameObject lastKnownLight;
	public bool isCrawling, isFleeing, fleeCooldown, isHeartCooldown;
	public float currentFear, currentTimeScream, scareInc, shakeTime, cooldown, heartBeatPitch, heartCooldown;
	public float scareDec;
    
	private NavMeshAgent navAgent;
	private ThirdPersonCharacter thirdPChar;
	private ThirdPersonUserControl thirdPControl;
	private PossessionMaster posMast;
	private AmplifyColorEffect ampColor;
	private AudioSource heartBeat;
	
	// Use this for initialization
	protected override void Start ()
	{
		base.Start ();
		//Pearl's variable intialization
		scaredState = state.NotScared;
		isCrawling = isFleeing = fleeCooldown = isHeartCooldown = false;
		currentFear = currentTimeScream = shakeTime = cooldown = heartCooldown = 0.0f;
		scareInc = SCARERATE;
		navAgent = GetComponent<NavMeshAgent> ();
		thirdPChar = GetComponent<ThirdPersonCharacter> ();
		thirdPControl = GetComponent<ThirdPersonUserControl> ();
		posMast = GameObject.Find ("Possession Master").GetComponent<PossessionMaster> ();
		ampColor = prisonerCamera.gameObject.transform.GetChild(0).GetChild(0).GetComponent<AmplifyColorEffect> ();
		heartBeat = GetComponent<AudioSource> ();
		heartBeatPitch = heartBeat.pitch;
	}
	
	// Update is called once per frame
	protected override void Update ()
	{
		base.Update ();
		if (PossessionMaster.CurrentlyPossesing == gameObject) {
			thirdPControl.enabled = true;
		}
		toggleCrawl ();
		detectLight ();
		fearManager ();
		stateManager ();
		fleeManager ();
		if (isFleeing) {
			thirdPChar.Move (navAgent.desiredVelocity, false, false, transform.forward);
		}
	}

	//fearmanager controls fear incrementation and decrementation
	private void fearManager ()
	{
		currentFear += Time.deltaTime * scareInc;
		if (currentFear > 0.0f && currentFear - Time.deltaTime * scareDec > 0.0f)
			currentFear -= Time.deltaTime * scareDec;
		else
			currentFear = 0.0f;
		
		FearMeter.updateMeter (currentFear / MAXFEAR);
		FearState.updateFearState (scaredState.ToString ());
	}

	//stateManager correlates fear state with currentFear and controls what happens during which state
	private void stateManager ()
	{
		if (currentFear < 0.0f) {
			currentFear = 0.0f;
			scaredState = state.NotScared;
		} else if (currentFear == 0.0f)
			scaredState = state.NotScared;
		else if (currentFear > 0.0f && currentFear <= MAXFEAR * scaredPercentage)
			scaredState = state.Scared;
		else if (currentFear > MAXFEAR * scaredPercentage && currentFear <= MAXFEAR)
			scaredState = state.Terrified;
		else if (currentFear > MAXFEAR)
			scaredState = state.Paralyzed;
		
		switch (scaredState) {
		case state.NotScared:
		{
			if (heartBeat.enabled && !isHeartCooldown){

			}
			if (heartBeat.enabled) {
				heartBeat.enabled = false;
				heartBeat.pitch = heartBeatPitch;
			}
			break;
		}
		case state.Scared:
		{
			if (!heartBeat.enabled && !fleeCooldown && !isFleeing) {
				heartBeat.enabled = true;
			}
			heartBeat.pitch = currentFear/MAXFEAR + heartBeatPitch;
			break;
		}
		case state.Terrified:	//If in terrified state, speed/2, jump/2 and turn on amplify color on fade
		{
			thirdPChar.MoveSpeedMultiplier = 0.5f;
			thirdPChar.JumpPower = 6.0f;
			if (!ampColor.enabled) {
				ampColor.enabled = true;
			}
			ampColor.BlendAmount = (currentFear / (MAXFEAR - (MAXFEAR * scaredPercentage))) * 1.0f;
			if (!heartBeat.enabled && !fleeCooldown && !isFleeing) {
				heartBeat.enabled = true;
			}
			heartBeat.pitch = currentFear/MAXFEAR + heartBeatPitch;
			break;
		}
		case state.Paralyzed:	//If in paralyzed state, push into astral form and start fleeing if timer is up
		{
			scareInc = 0.0f;
			if (PossessionMaster.CurrentlyPossesing && PossessionMaster.CurrentlyPossesing.gameObject == gameObject) {
				StartCoroutine (posMast.enterAstral ());
			}
			if (isFleeing) {
				currentTimeScream = 0.0f;
			} else {
				currentTimeScream += Time.deltaTime;
				thirdPChar.Move (Vector3.zero, true, false, transform.forward);
			}
			if (currentTimeScream >= MAXTIMESCREAM) {
				fleeToLight ();
			}
			break;
		}
		}
		//If speed was changed in terrified state, but is no longer in terrified state, return original speed
		if (thirdPChar.MoveSpeedMultiplier != 1.0f && scaredState != state.Terrified) {
			thirdPChar.MoveSpeedMultiplier = 1.0f;
		}
		//If jump was changed in terrified state, but is no longer in terrified state, return original jump
		if (thirdPChar.JumpPower != 6.0f && scaredState != state.Terrified) {
			thirdPChar.JumpPower = 12.0f;
		}
		//If amplify color was enabled during terrifed state, but is no longer in terrifed state, disable amplified color
		if (ampColor.enabled && scaredState != state.Terrified) {
			ampColor.BlendAmount = 0.0f;
			ampColor.enabled = false;
		}
	}

	//detectLight checks if Pearl is in light and calculates scareDec
	private void detectLight ()
	{
		List<GameObject> lightList = new List<GameObject> ();
		float radius, currRadius;
		
		lightList = LightManager.getEncompassingLight (gameObject);
		radius = currRadius = 0.0f;
		scareDec = 0.0f;
		if (lightList.Count > 0) {
			lightList.ForEach (delegate(GameObject i) {
				radius = LightManager.getRadius(i);
				currRadius = Vector2.Distance (new Vector2 (i.transform.position.x, i.transform.position.z),
				                               new Vector2 (gameObject.transform.position.x, gameObject.transform.position.z));
				//Debug.Log("rad: " + radius + "cur: " + currRadius);
				
				if (currRadius > 0.0f && currRadius <= radius * lightDec1) {
					scareDec += (SCAREDECRATE  * i.light.intensity / 5.0f );
				} else if (currRadius > radius * lightDec1 && currRadius <= radius * lightDec2) {
					scareDec += (SCAREDECRATE  * i.light.intensity / 5.0f ) / 2.0f;
				}
				else if (currRadius > radius * lightDec2 && currRadius <= radius) {
					if (scaredState == state.Scared && (currentFear >= (MAXFEAR * scaredPercentage - 0.5f))) {
						scareInc = 0.0f;
					} else if (scaredState == state.Terrified || scaredState == state.Paralyzed) {
						scareDec += 1.0f;
					}
					scareDec += 0.5f;
				}
				lastKnownLight = i;
			});
		}
	}

	//fleeManager controls post-operations for fleeing and cooldown (animations)
	private void fleeManager () {
		//If is fleeing and not in paralyzed state, disable navAgent and turn on fleeCooldown
		if (isFleeing && scaredState != state.Paralyzed) {
			navAgent.enabled = false;
			isFleeing = false;
			fleeCooldown = true;
			currentTimeScream = 0.0f;
		}
		//If no longer fleeing and scareInc has been changed, return to original scareInc
		if (!isFleeing && scareInc == 0.0f) {
			scareInc = SCARERATE;
		}
		//If fleeCooldown, set timer, crouch when not paralyzed. If cooldown over, disable fleeCooldown and stop crouching.
		if (fleeCooldown) {
			cooldown += Time.deltaTime;
			thirdPChar.Move (Vector3.zero, true, false, transform.forward);
			if (cooldown >= 1.0f) {
				fleeCooldown = false;
				cooldown = 0.0f;
				thirdPChar.Move (Vector3.zero, false, false, transform.forward);
			}
		}
	}

	//fleeToLight controls Pearl to flee to lastKnownLight
	private bool fleeToLight ()
	{
		if (lastKnownLight && lastKnownLight.light.enabled) {
			thirdPControl.enabled = false;
			navAgent.enabled = true;
			navAgent.destination = lastKnownLight.transform.position;
			isFleeing = true;
			return true;
		}
		return false;
	}

	private void fadeHeartBeat () {
		if (!isHeartCooldown)
			isHeartCooldown = true;
		heartCooldown += Time.deltaTime;
		if (heartCooldown >= 1.0f) {
			heartBeat.pitch = heartBeatPitch;
			heartBeat.enabled = false;
			heartBeat.volume = 1.0f;
			heartCooldown = 0.0f;
		}
		heartBeat.volume -= (Time.deltaTime * 5.0f);
	}

	//not being used
	private void shakeCamera (float magnitude)
	{
		Camera.main.transform.Rotate (new Vector3 (0.0f, 0.0f, Random.Range (-magnitude, magnitude)));
	}

	private void toggleCrawl ()
	{
		if (isCrawling && Input.GetKeyDown (KeyCode.C))
			isCrawling = false;
		else if (!isCrawling && Input.GetKeyDown (KeyCode.C))
			isCrawling = true;
	}
}
