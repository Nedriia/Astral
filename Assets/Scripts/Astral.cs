using UnityEngine;
using Holoville.HOTween;
using System.Collections;

public class Astral : MonoBehaviour, Possessable {

    public PossessionMaster possMess;

    private Prisoner currentlyViewing;
	private Animation eyes;
    //used for disabling and enabling movement
    private SimpleMouseRotator mouseRotatorLR, mouseRotatorUD;
    private FirstPersonCharacter characterMover;
    private bool disabledOnSceneStart = true;

	// Use this for initialization
	void Start () {
        disabledOnSceneStart = false;
		eyes = gameObject.GetComponent<Animation>();
        possMess = GameObject.Find("Possession Master").GetComponent<PossessionMaster>();
        mouseRotatorLR = gameObject.GetComponent<SimpleMouseRotator>();
        mouseRotatorUD = gameObject.GetComponentsInChildren<SimpleMouseRotator>()[1];
        characterMover = gameObject.GetComponentInChildren<FirstPersonCharacter>();
	}

    //if julia was disabled on the screen start we never get her component references
    //adding them here will grab them when we enable her
    void OnEnable() {
        if (disabledOnSceneStart) {
            eyes = gameObject.GetComponent<Animation>();
            mouseRotatorLR = gameObject.GetComponent<SimpleMouseRotator>();
            mouseRotatorUD = gameObject.GetComponentInChildren<SimpleMouseRotator>();
            characterMover = gameObject.GetComponentInChildren<FirstPersonCharacter>();
            disabledOnSceneStart = false;
        }
    }

	// Update is called once per frame
	void Update () {
        Debug.DrawRay(transform.position, transform.forward * 1, Color.cyan);
	}

    //true for in, false for out, return amount of time to wait
    public float bodyTransition(bool entering) {
        float waitTime = 0;
        if (entering) {
            //eyes.Play();
            //waitTime = eyes["Eyes Open"].length;
        } else {
            //eyes.Play("Eyes Close");
            //waitTime = eyes["Eyes Close"].length;
        }
        return waitTime;
    }

    public Prisoner CurrentlyViewing {
        get { return currentlyViewing; }
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
