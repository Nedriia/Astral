using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pearl : Prisoner
{
	public float MAXFEAR = 6.0f;
	public float MAXTIMESCREAM = 1.0f;
	public float SCARERATE = 1.0f;
	public float SCAREDECRATE = 3.0f;
	public float scaredPercentage = 0.4f;
	public float terrifiedPercentage = 0.6f;
	public float lightDec1 = 0.3f;
	public float lightDec2 = 0.5f;
	public float lightDec3 = 0.8f;

	private enum state
	{
		NotScared,
		Scared,
		Terrified,
		Paralyzed}
	;
	private state scaredState;
	private GameObject lastKnownLight;
	private bool isCrawling, isFleeing, fleeCooldown, cameraRot;
	public float currentFear, currentTimeScream, scareInc, shakeTime, cooldown;
	public float scareDec;
	private NavMeshAgent navAgent;
	private ThirdPersonCharacter thirdPChar;
	private ThirdPersonUserControl thirdPControl;
	private Quaternion originRot;
	private PossessionMaster posMast;

	// Use this for initialization
	protected override void Start ()
	{
		base.Start ();
		//Pearl's variable intialization
		scaredState = state.NotScared;
		isCrawling = isFleeing = fleeCooldown = cameraRot = false;
		currentFear = currentTimeScream = shakeTime = cooldown = 0.0f;
		scareInc = SCARERATE;
		navAgent = GetComponent<NavMeshAgent> ();
		thirdPChar = GetComponent<ThirdPersonCharacter> ();
		thirdPControl = GetComponent<ThirdPersonUserControl> ();
		posMast = GameObject.Find ("Possession Master").GetComponent<PossessionMaster> ();
	}
	
	// Update is called once per frame
	protected override void Update ()
	{
		base.Update ();
		if (PossessionMaster.CurrentlyPossesing == gameObject) {
			originRot = this.prisonerCamera.transform.GetChild (0).GetChild (0).localRotation;
			thirdPControl.enabled = true;
		}
		toggleCrawl ();
		detectLight ();
		fearManager ();
		stateManager ();
		fleeManager ();
	}

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
				break;
			}
		case state.Scared:
			{
			//call sounds
			break;
			}
		case state.Terrified:
			{
				thirdPChar.MoveSpeedMultiplier = 0.5f;
				shakeTime += Time.deltaTime;
				if (shakeTime >= Random.Range (0.1f, 0.3f)) {
					shakeTime = 0.0f;
					shakeCamera (4.0f);
				}
				cameraRot = true;
				break;
			}
		case state.Paralyzed:
			{
				Camera.main.transform.rotation = originRot;
				scareInc = 0.0f;
				if (isFleeing) {
					currentTimeScream = 0.0f;
				} else {
					currentTimeScream += Time.deltaTime;
				}
				if (currentTimeScream >= MAXTIMESCREAM) {
					fleeToLight ();
				}
				break;
			}
		}
		if (thirdPChar.MoveSpeedMultiplier != 1.0f && scaredState != state.Terrified) {
			thirdPChar.MoveSpeedMultiplier = 1.0f;
		}
		if (cameraRot && scaredState != state.Terrified) {
			Camera.main.transform.rotation = originRot;
			cameraRot = false;
		}
	}

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
				Debug.Log("rad: " + radius + "cur: " + currRadius);
			
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

	private void shakeCamera (float magnitude)
	{
		Camera.main.transform.Rotate (new Vector3 (0.0f, 0.0f, Random.Range (-magnitude, magnitude)));
	}

	private bool fleeToLight ()
	{
		if (lastKnownLight && lastKnownLight.light.enabled) {
			if (PossessionMaster.CurrentlyPossesing && PossessionMaster.CurrentlyPossesing.gameObject == gameObject) {
				StartCoroutine (posMast.enterAstral ());
			}
			thirdPControl.enabled = false;
			navAgent.enabled = true;
			navAgent.destination = lastKnownLight.transform.position;
			isFleeing = true;
			return true;
		}
		return false;
	}
	
	private void fleeManager () {
		if (isFleeing) {
			thirdPChar.Move (navAgent.desiredVelocity, false, false, transform.forward);
			if (scareDec > SCARERATE) {
				navAgent.enabled = false;
				isFleeing = false;
				fleeCooldown = true;
			}
		}
		if (scaredState != state.Paralyzed && scareInc == 0.0f) {
			scareInc = SCARERATE;
		}
		if (fleeCooldown) {
			cooldown += Time.deltaTime;
			thirdPChar.Move (Vector3.zero, false, false, transform.forward);
			if (cooldown >= 1.0f) {
				fleeCooldown = false;
				cooldown = 0.0f;
			}
		}
	}

	private void toggleCrawl ()
	{
		if (isCrawling && Input.GetKeyDown (KeyCode.C))
			isCrawling = false;
		else if (!isCrawling && Input.GetKeyDown (KeyCode.C))
			isCrawling = true;
	}
}
