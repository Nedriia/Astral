using UnityEngine;
using Holoville.HOTween;
using System.Collections;

public class Astral : MonoBehaviour, Possessable {

    private GameObject currentlyViewing;
	private Animation eyes;
    private bool eyesOpen;
    //used for disabling and enabling movement
    private SimpleMouseRotator mouseRotatorLR, mouseRotatorUD;
    private FirstPersonCharacter characterMover;
    private bool disabledOnSceneStart = true;

	// Use this for initialization
	void Start () {
        disabledOnSceneStart = false;
        eyesOpen = true;
		eyes = gameObject.GetComponent<Animation>();
        mouseRotatorLR = gameObject.GetComponent<SimpleMouseRotator>();
        mouseRotatorUD = gameObject.GetComponentInChildren<SimpleMouseRotator>();
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

	}

    //true for in, false for out, return amount of time to wait
    public float bodyTransition(bool entering) {
        float waitTime = 0;
        if ( entering ) {
            eyes.Play();
            eyesOpen = false;
            waitTime = eyes["Eyes Open"].length;
        } else {
            eyes.Play("Eyes Close");
            eyesOpen = true;
            waitTime = eyes["Eyes Close"].length;
        }
        return waitTime;
    }


    private void addPrisoner( GameObject prisoner ) {

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
        mouseRotatorLR.enabled = false;
        mouseRotatorUD.enabled = false;
        characterMover.enabled = false;
    }
}
