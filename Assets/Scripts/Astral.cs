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
	// Use this for initialization
	void Start () {
        eyesOpen = true;
		eyes = gameObject.GetComponent<Animation>();
        mouseRotatorLR = gameObject.GetComponent<SimpleMouseRotator>();
        mouseRotatorUD = gameObject.GetComponentInChildren<SimpleMouseRotator>();
        characterMover = gameObject.GetComponentInChildren<FirstPersonCharacter>();
	}
	
	// Update is called once per frame
	void Update () {

	}

    //true for in, false for out
    public void bodyTransition( bool entering ) {
            if ( entering ) {
                eyes.Play();
                eyesOpen = false;
            } else {
                eyes.Play("Eyes Open");
                eyesOpen = true;
            }
    }


    private void addPrisoner( GameObject prisoner ) {

    }

    private void enterAstralForm() {

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
