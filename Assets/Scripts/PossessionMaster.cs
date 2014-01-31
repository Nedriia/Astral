using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PossessionMaster : MonoBehaviour {

    public Astral Julia;
    public FreeLookCam prisonerCamera;
    public Prisoner startingPossessedPrisoner;

    private List<Prisoner> prisonerInventory;
    private Prisoner currentlyPossessing;
    private static bool astralForm;
	// Use this for initialization
	void Start () {
        //we are starting in a prisoner
        if (startingPossessedPrisoner != null) {
            astralForm = true;  //this looks decieving, it will be changed to false in the swap function
            StartCoroutine(swap(startingPossessedPrisoner));
        //we are staring in Julia
        } else {
            astralForm = true;
        }
	}
	
	// Update is called once per frame
	void Update () {
	}

    public bool AstralForm {
        get { return astralForm; }
        set { astralForm = value; }
    }

    public List<Prisoner> getInventory() { 
        return prisonerInventory; 
    }

    //enters astral form when in prisoner
    public IEnumerator enterAstral() {
        //Prisoner to Julia
        yield return new WaitForSeconds(transitionP(currentlyPossessing, false));
        prisonerCamera.gameObject.SetActive(false);
        yield return new WaitForSeconds(transitionJ(true));
        Julia.startControlling();
    }

    //swaps to the specified prisoner
    public IEnumerator swap( Prisoner curPrisoner ) {
        //Julia to prisoner
        if ( astralForm && ( currentlyPossessing == null ) ) {
            //wait for julia out animation to finish
            yield return new WaitForSeconds(transitionJ(false));
            //wait for prisoner in animation to finish
            yield return new WaitForSeconds(transitionP(curPrisoner, true));
            curPrisoner.startControlling();
        //Prisoner to Prisoner
        } else if ( !astralForm && ( currentlyPossessing != null) ) {
            //wait for prisoner out animation to finish
            yield return new WaitForSeconds(transitionP(currentlyPossessing, false));
            //wait for in animation to finish
            yield return new WaitForSeconds(transitionP(curPrisoner, true));
            curPrisoner.startControlling();
        }
    }

    //transtions in or out of julia
    private float transitionJ( bool enter ) {
        float waitTime = 0;
        if (enter) {
            Julia.gameObject.SetActive(true);
            waitTime = Julia.bodyTransition(true);
            astralForm = true;
        } else {
            Julia.stopControlling();
            waitTime = Julia.bodyTransition(false);
            astralForm = false;
            Julia.gameObject.SetActive(false);
        }
        return waitTime;
    }

    //transtions in or out of specified prisoner
    private float transitionP( Prisoner curPrisoner, bool enter ) {
        float waitTime = 0;
        if (enter) {
            //enable third person camera rig and set on target
            prisonerCamera.gameObject.SetActive(true);
            prisonerCamera.SetTarget(curPrisoner.gameObject.transform);
            waitTime = curPrisoner.bodyTransition(true);
            currentlyPossessing = curPrisoner;
        } else {
            curPrisoner.stopControlling();
            waitTime = curPrisoner.bodyTransition(false);
            currentlyPossessing = null;
        }
        return waitTime;
    }

}