using UnityEngine;
using Holoville.HOTween;
using System.Collections;
using System.Collections.Generic;
public class Astral : MonoBehaviour {

    private GameObject currentlyViewing;
	private Animation eyes;
    private bool eyesOpen;

	// Use this for initialization
	void Start () {
        eyesOpen = true;
		eyes = gameObject.GetComponent<Animation>();
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
}
