using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PossessionMaster : MonoBehaviour {

    public Astral Julia;
    public FreeLookCam prisonerCamera;
    public ProtectCameraFromWallClip protFrmWalls;
    public Prisoner startingPosPrisoner;

    private List<Prisoner> prisonerInventory;
    private Prisoner currentlyPossessing;
    private static bool astralForm;
	// Use this for initialization
	void Start () {
        Julia = GameObject.Find("Julia").GetComponent<Astral>();
        prisonerCamera = GameObject.Find("Astral Camera Rig").GetComponent<FreeLookCam>();
        protFrmWalls = GameObject.Find("Astral Camera Rig").GetComponent<ProtectCameraFromWallClip>();
        prisonerInventory = new List<Prisoner>();
        //we are starting in a prisoner
        if (startingPosPrisoner != null) {
            StartCoroutine(possesionStart(true));
        //we are staring in Julia
        } else {
            StartCoroutine(possesionStart(false));
        }
	}
	
	// Update is called once per frame
	void Update () {
	}

    public static bool AstralForm {
        get { return astralForm; }
    }

    public List<Prisoner> getInventory() { 
        return prisonerInventory; 
    }

    //enters astral form when in prisoner
    public IEnumerator enterAstral() {
        //Prisoner to Julia
        Vector3 juliaNewPosition = new Vector3(currentlyPossessing.transform.position.x,currentlyPossessing.transform.position.y+0.2f,currentlyPossessing.transform.position.z+0.5f);
        //Vector3 juliaNewPosition = currentlyPossessing.transform.position;
        yield return new WaitForSeconds(transitionP(currentlyPossessing, false));
        prisonerCamera.gameObject.SetActive(false);
        Julia.gameObject.SetActive(true);
        Julia.transform.position = juliaNewPosition; //this will need to be edited
        yield return new WaitForSeconds(transitionJ(true));
        Julia.startControlling();
    }

    //swaps to the specified prisoner
    public IEnumerator swap( Prisoner curPrisoner ) {
        //Julia to prisoner
        if ( astralForm && (currentlyPossessing == null)) {
            //wait for julia out animation to finish
            yield return new WaitForSeconds(transitionJ(false));
            Julia.gameObject.SetActive(false);
            //wait for prisoner in animation to finish
            yield return new WaitForSeconds(transitionP(curPrisoner, true));
            curPrisoner.startControlling();
        //Prisoner to Prisoner
        } else if (!astralForm && (currentlyPossessing != null)) {
            //wait for prisoner out animation to finish
            yield return new WaitForSeconds(transitionP(currentlyPossessing, false));
            //wait for in animation to finish
            yield return new WaitForSeconds(transitionP(curPrisoner, true));
            curPrisoner.startControlling();
        }
    }

    //transtions in or out of julia
    private float transitionJ(bool enter) {
        float waitTime = 0;
        if (enter) {
            waitTime = Julia.bodyTransition(true);
            astralForm = true;
        } else {
            Julia.stopControlling();
            waitTime = Julia.bodyTransition(false);
            astralForm = false;
        }
        return waitTime;
    }

    //transtions in or out of specified prisoner
    private float transitionP(Prisoner curPrisoner, bool enter) {
        float waitTime = 0;
        if (enter) {
            //enable third person camera rig and set on target
            prisonerCamera.gameObject.SetActive(true);
            prisonerCamera.gameObject.transform.position = curPrisoner.transform.position;
            
            //must disable than enable walls script to set zoom level
            //protFrmWalls.enabled = false;
            //set our current prisoners camra pivot positioning and zoom level
            Debug.Log(prisonerCamera.gameObject.transform.childCount);
            Vector3 pivotPosition = prisonerCamera.gameObject.transform.GetChild(0).localPosition;
            Vector3 cameraZoom = prisonerCamera.gameObject.transform.GetChild(0).GetChild(0).localPosition;
            protFrmWalls.closestDistance = curPrisoner.camZoom;
            //cameraZoom.z = curPrisoner.camZoom;
            pivotPosition.y = curPrisoner.camPivVert;
            pivotPosition.x = curPrisoner.camPivHor;

            //prisonerCamera.gameObject.transform.GetChild(0).GetChild(0).localPosition = cameraZoom;
            prisonerCamera.gameObject.transform.GetChild(0).localPosition = pivotPosition;

            //protFrmWalls.enabled = true;
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

    private IEnumerator possesionStart(bool startAsPrisoner) {
        if (startAsPrisoner) {
            Julia.gameObject.SetActive(false);
            astralForm = false;
            yield return new WaitForSeconds(transitionP(startingPosPrisoner, true));
            currentlyPossessing.startControlling();
        } else {
            astralForm = true;
            prisonerCamera.gameObject.SetActive(false);
            yield return new WaitForSeconds(transitionJ(true));
            Julia.startControlling();
        }
    }
}