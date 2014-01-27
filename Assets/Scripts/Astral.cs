using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Astral : MonoBehaviour {

    public List<GameObject> prisonerInventory;

    private GameObject currentlyPossessing;
    private GameObject currentlyViewing;
    private NavMeshAgent swappingTo;
    private MotionBlur mb;
    private ColorCorrectionCurves ccc;
    private SimpleMouseRotator sMR;
    private FirstPersonHeadBob headBob;

	// Use this for initialization
	void Start () {
        swappingTo = gameObject.GetComponent<NavMeshAgent>();
        mb = gameObject.GetComponentInChildren<MotionBlur>();
        ccc = gameObject.GetComponentInChildren<ColorCorrectionCurves>();
        sMR = gameObject.GetComponent<SimpleMouseRotator>();
        headBob = gameObject.GetComponent<FirstPersonHeadBob>();
	}
	
	// Update is called once per frame
	void Update () {
        swap();
	}

    private void swap() {
        if ( Input.GetKeyDown( KeyCode.Q ) ) {
            swappingTo.enabled = true;
            mb.enabled = true;
            ccc.enabled = true;
            headBob.enabled = false;
            sMR.enabled = false;
            swappingTo.SetDestination(prisonerInventory[0].transform.position);
        }

        if (swappingTo.enabled) {
            if ( swappingTo.remainingDistance <= 2 && !swappingTo.pathPending ) {

                swappingTo.enabled = false;
                mb.enabled = false;
                ccc.enabled = false;
                sMR.enabled = true;
                headBob.enabled = true;

            }
        }
    }

    private bool inAstralForm() {

        return true;
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
