using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PossessionMaster : MonoBehaviour {

    public Astral Julia;
    public FreeLookCam prisonerCamera;

    private List<Prisoner> prisonerInventory;
    private Prisoner currentlyPossessing;
    private static bool astralForm;
	// Use this for initialization
	void Start () {
	    
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
    public void enterAstral() {
        //Prisoner to Julia
        transitionP(currentlyPossessing, false);
        transitionJ(true);
    }

    //swaps to the specified prisoner
    public void swap( Prisoner curPrisoner ) {
        //Julia to prisoner
        if ( astralForm && ( currentlyPossessing == null ) ) {
            transitionJ(false);
            transitionP(curPrisoner, true);
        //Prisoner to Prisoner
        } else if ( !astralForm && ( currentlyPossessing != null) ) {
            transitionP(currentlyPossessing, false);
            transitionP(curPrisoner, true);
        }
    }

    //transtions in or out of julia
    private void transitionJ( bool enter ) {
        
        if (enter) {
            Julia.gameObject.SetActive(true);
            Julia.bodyTransition(true);
            Julia.startControlling();
        } else {
            Julia.stopControlling();
            Julia.bodyTransition(false);
            Julia.gameObject.SetActive(false);
        }
    }

    //transtions in or out of specified prisoner
    private void transitionP( Prisoner curPrisoner, bool enter ) {
        if (enter) {
            currentlyPossessing = curPrisoner;
            curPrisoner.bodyTransition(true);
            //enable third person camera rig and set on target
            prisonerCamera.enabled = true;
            prisonerCamera.SetTarget(curPrisoner.gameObject.transform);
            curPrisoner.startControlling();
        } else {
            currentlyPossessing = null;
            curPrisoner.bodyTransition(false);
            //disable third person camera rig
            prisonerCamera.enabled = false;
            curPrisoner.stopControlling();
        }
    }
}