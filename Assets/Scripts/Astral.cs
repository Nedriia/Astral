using UnityEngine;
using Holoville.HOTween;
using System.Collections;

public class Astral : MonoBehaviour, Possessable {

    public PossessionMaster possMess;
    public Texture2D flash;

    private Prisoner currentlyViewing;
	private Animation anim;
    //used for disabling and enabling movement
    private SimpleMouseRotator mouseRotatorLR, mouseRotatorUD;
    private FirstPersonCharacter characterMover;
    private bool disabledOnSceneStart = true;
    private static GameObject currentlyTargeting;

	// Use this for initialization
	void Start () {
        disabledOnSceneStart = false;
		anim = gameObject.GetComponent<Animation>();
        possMess = GameObject.Find("Possession Master").GetComponent<PossessionMaster>();
        mouseRotatorLR = gameObject.GetComponent<SimpleMouseRotator>();
        mouseRotatorUD = gameObject.GetComponentsInChildren<SimpleMouseRotator>()[1];
        characterMover = gameObject.GetComponentInChildren<FirstPersonCharacter>();
	}

    //if julia was disabled on the screen start we never get her component references
    //adding them here will grab them when we enable her
    void OnEnable() {
        if (disabledOnSceneStart) {
            anim = gameObject.GetComponent<Animation>();
            mouseRotatorLR = gameObject.GetComponent<SimpleMouseRotator>();
            mouseRotatorUD = gameObject.GetComponentInChildren<SimpleMouseRotator>();
            characterMover = gameObject.GetComponentInChildren<FirstPersonCharacter>();
            disabledOnSceneStart = false;
        }
    }

	// Update is called once per frame
	void Update () {
        RaycastHit hit;
		LayerMask ignoreLights = (1 << 9) | (1 << 10);
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);

		if(Physics.SphereCast(ray, 0.5f, out hit, Mathf.Infinity, ignoreLights.value)){
			currentlyTargeting = hit.collider.gameObject;
		}

	}

    //true for in, false for out, return amount of time to wait
    public float bodyTransition(bool entering) {
        float waitTime = 0;
        if (entering) {
			gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<AmplifyColorEffect>().LutTexture = flash;
            anim.Play();
			waitTime = anim["FlashIn"].length;
        } else {
			gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<AmplifyColorEffect>().LutTexture = flash;
			anim.Play("FlashOut");
			waitTime = anim["FlashOut"].length;
        }
        return waitTime;
    }

    public Prisoner CurrentlyViewing {
        get { return currentlyViewing; }
    }

    public static GameObject CurrentlyTargeting {
        get { return currentlyTargeting; }
    }

    public void addPrisoner() {
        possMess.getInventory().Add(currentlyViewing);
    }

    public void addPrisoner( Prisoner prisoner ) {
        possMess.getInventory().Add(prisoner);
    }

    private void astralWalls( bool wallTransparent ) {

    }

    private void permeateSolids(bool goThroughThings) {

    }

    private void readThoughts() {

    }

    private void getHint() {

    }

    public void startControlling() {
        mouseRotatorLR.enabled = true;
        mouseRotatorUD.enabled = true;
        characterMover.enabled = true;
    }

    public void stopControlling() {
        gameObject.rigidbody.velocity = Vector3.zero;
        mouseRotatorLR.enabled = false;
        mouseRotatorUD.enabled = false;
        characterMover.enabled = false;
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Prisoner")) {
            currentlyViewing = other.gameObject.GetComponent<Prisoner>();
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Prisoner")) {
            currentlyViewing = null;
        }
    }
}
