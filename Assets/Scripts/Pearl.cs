using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pearl : Prisoner {

	public float MAXFEAR = 6.0f;
	public float MAXTIMESCREAM = 1.0f;
	public float SCARERATE = 1.0f;
	public float scaredPercentage = 0.4f;
	public float terrifiedPercentage = 0.6f;

	private enum state {NotScared, Scared, Terrified, Paralyzed};
	private state scaredState;
	private GameObject lastKnownLight;
	private bool isCrawling, isFleeing;
	private float currentFear, currentTimeScream, scareInc, shakeTime;
	public float scareDec;
	private NavMeshAgent navAgent;
	private ThirdPersonCharacter thirdPChar;
	private ThirdPersonUserControl thirdPControl;
	private Quaternion originRot;
	private PossessionMaster posMast;

	// Use this for initialization
	protected override void Start () {
		base.Start();
		//Pearl's variable intialization
		scaredState = state.NotScared;
		isCrawling = false;
		isFleeing = false;
		currentFear = 0.0f;
		currentTimeScream = 0.0f;
		scareInc = SCARERATE;
		shakeTime = 0.0f;
		navAgent = GetComponent<NavMeshAgent>();
		thirdPChar = GetComponent<ThirdPersonCharacter>();
		thirdPControl = GetComponent<ThirdPersonUserControl>();
		posMast = GameObject.Find ("Possession Master").GetComponent<PossessionMaster>();


		//Inherited variable re-initialization
		//speed *= 1.5f;
		//jumpVelocity *= 1.5f;
	}
	
	// Update is called once per frame
	protected override void Update () {
		//this.prisonerCamera.transform.GetChild(0).GetChild(0).localRotation
		base.Update ();
		if (PossessionMaster.CurrentlyPossesing == gameObject) {
			originRot = this.prisonerCamera.transform.GetChild(0).GetChild(0).localRotation;
		}
		toggleCrawl();
		detectLight();
		fearManager();
		stateManager();
	}

	private void fearManager() {
		currentFear += Time.deltaTime * scareInc;
		if (currentFear > 0.0f && currentFear - Time.deltaTime * scareDec > 0.0f)
			currentFear -= Time.deltaTime * scareDec;
		else
			currentFear = 0.0f;

		FearMeter.updateMeter (currentFear/MAXFEAR);
		FearState.updateFearState (scaredState.ToString());
	}

	private void stateManager() {
		if (currentFear < 0.0f) {
			currentFear = 0.0f;
			scaredState = state.NotScared;
		}
		else if (currentFear == 0.0f)
			scaredState = state.NotScared;
		else if (currentFear > 0.0f && currentFear <= MAXFEAR * scaredPercentage)
			scaredState = state.Scared;
		else if (currentFear > MAXFEAR * scaredPercentage && currentFear <= MAXFEAR * terrifiedPercentage)
			scaredState = state.Terrified;
		else if (currentFear > MAXFEAR)
			scaredState = state.Paralyzed;
		switch (scaredState)
		{
			case state.NotScared: {
				break;
			}
			case state.Scared: {
				Camera.main.transform.rotation = originRot;
				break;
			}
			case state.Terrified: {
				if (isFleeing) {
					navAgent.enabled = false;
					isFleeing = false;
				}
				if (scareInc == 0.0f) {
					scareInc = SCARERATE;
				}
				thirdPChar.MoveSpeedMultiplier = 0.5f;
				shakeTime += Time.deltaTime;
				if (shakeTime >= Random.Range(0.1f, 0.3f)) {
					shakeTime = 0.0f;
					shakeCamera(4.0f);
				}
				break;
			}
			case state.Paralyzed: {
				Camera.main.transform.rotation = originRot;
				scareInc = 0.0f;
				if (isFleeing) {
					currentTimeScream = 0.0f;
				fleeToLight();
				}
				else {
					currentTimeScream += Time.deltaTime;
				}
				if (currentTimeScream >= MAXTIMESCREAM) {
					fleeToLight();
				}
				break;
			}
		}
	}

	private void detectLight() {
		List<GameObject> lightList = new List<GameObject>();
		float radius, currRadius, yPos;

		lightList = LightManager.getEncompassingLight(gameObject);
		radius = currRadius = yPos = 0.0f;
		scareDec = 0.0f;
		if (lightList.Count > 0) {
			lightList.ForEach(delegate(GameObject i) {
				if (i.light.type == LightType.Spot)
				{
					yPos = i.transform.position.y * 1.1f;
					radius = i.light.spotAngle / (i.light.range - yPos);
				} else if (i.light.type == LightType.Point)
				{
					yPos = i.transform.position.y;
					radius = (i.light.range / yPos) * (i.light.intensity * 0.3f);
				}
				currRadius = Vector2.Distance(new Vector2(i.transform.position.x, i.transform.position.z),
				                              new Vector2(gameObject.transform.position.x, gameObject.transform.position.z));
				//Debug.Log("rad: " + radius + "cur: " + currRadius);
				scareDec += (radius - currRadius)*i.light.intensity/1;
				lastKnownLight = i;
			});
		}
	}

	private void shakeCamera (float magnitude) {
		Camera.main.transform.Rotate(new Vector3(0.0f, 0.0f, Random.Range(-magnitude, magnitude)));
	}

	private bool fleeToLight () {
		if (lastKnownLight && lastKnownLight.light.enabled) {
			if (PossessionMaster.CurrentlyPossesing && PossessionMaster.CurrentlyPossesing.gameObject == gameObject) {
				StartCoroutine(posMast.enterAstral());
			}
			//thirdPControl.enabled = false;
			navAgent.enabled = true;
			navAgent.destination = lastKnownLight.transform.position;
			thirdPChar.Move( navAgent.desiredVelocity, false, false, lastKnownLight.transform.position );
			//navAgent.destination = new Vector3(lastKnownLight.transform.position.x, 0.0f, lastKnownLight.transform.position.z);
			isFleeing = true;
			return true;
		}
		return false;
	}

	private void toggleCrawl () {
		if (isCrawling && Input.GetKeyDown(KeyCode.C))
			isCrawling = false;
		else if (!isCrawling && Input.GetKeyDown(KeyCode.C))
			isCrawling = true;
	}
}
