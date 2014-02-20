using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;

public class PossessionMaster : MonoBehaviour {

    public Astral Julia;
    public FreeLookCam prisonerCamera;
    public Prisoner startingPosPrisoner;
    public float flyThroughSpeed, turnSpeed;

    private List<Prisoner> prisonerInventory;
    private static Prisoner currentlyPossessing;
    private static bool astralForm;
    private bool canSwap;

	// Use this for initialization
	void Start () {
        Julia = GameObject.Find("Julia").GetComponent<Astral>();
        prisonerCamera = GameObject.Find("Prisoner Camera Rig").GetComponent<FreeLookCam>();
        prisonerInventory = new List<Prisoner>();
        canSwap = true;
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
        Debug.Log(astralForm);
	}

    public bool CanSwap {
        get { return canSwap; }
    }
    public static bool AstralForm {
        get { return astralForm; }
    }
    public static Prisoner CurrentlyPossesing {
        get { return currentlyPossessing; }
    }
    public List<Prisoner> getInventory() { 
        return prisonerInventory; 
    }

    //enters astral form when in prisoner
    public IEnumerator enterAstral() {
        canSwap = false;
        //Prisoner to Julia
        Vector3 juliaNewPosition = currentlyPossessing.transform.position;
        CapsuleCollider prisonerCapsule = currentlyPossessing.GetComponent<CapsuleCollider>();
        Vector3 capsuleStart = currentlyPossessing.transform.position, capsuleEnd = Vector3.zero;
        capsuleStart.y += prisonerCapsule.height;
        capsuleEnd = capsuleStart;
        capsuleEnd.y -= prisonerCapsule.height;

        //make sure to spawn julia in a place where there is no collisions
        if (!Physics.CapsuleCast(capsuleStart, capsuleEnd, prisonerCapsule.radius / 1.5f, currentlyPossessing.transform.forward, 1)) {
            juliaNewPosition += currentlyPossessing.transform.forward;
            Debug.Log("forward");
        } else if (!Physics.CapsuleCast(capsuleStart, capsuleEnd, prisonerCapsule.radius / 1.5f, currentlyPossessing.transform.forward* -1, 1)) {
            juliaNewPosition += (currentlyPossessing.transform.forward * -1);
            Debug.Log("back");
        } else if (!Physics.CapsuleCast(capsuleStart, capsuleEnd, prisonerCapsule.radius / 1.5f, currentlyPossessing.transform.right, 1)) {
            juliaNewPosition += currentlyPossessing.transform.right;
            Debug.Log("right");
        } else if (!Physics.CapsuleCast(capsuleStart, capsuleEnd, prisonerCapsule.radius / 1.5f, currentlyPossessing.transform.right * -1, 1)) {
            juliaNewPosition += (currentlyPossessing.transform.right * -1);
            Debug.Log("left");
        }
        //make him her a little higher so she doesnt fall through the floor
        juliaNewPosition.y += 1;
        yield return new WaitForSeconds(transitionP(currentlyPossessing, false));
        prisonerCamera.gameObject.SetActive(false);
        Julia.gameObject.SetActive(true);
        Julia.transform.position = juliaNewPosition; 
        yield return new WaitForSeconds(transitionJ(true));
        Julia.startControlling();
        canSwap = true;
    }

    //swaps to the specified prisoner
    public IEnumerator swap( Prisoner curPrisoner ) {
        //Julia to prisoner
        if ( astralForm && (currentlyPossessing == null)) {
            canSwap = false;
            Vector3 turnDirection =  curPrisoner.gameObject.transform.position - Julia.gameObject.transform.position;
            turnDirection.Normalize();
            Julia.stopControlling();
            yield return StartCoroutine(HOTween.To(Julia.gameObject.transform, turnSpeed, "rotation", Quaternion.LookRotation(turnDirection)).WaitForCompletion());

            //Fly from current position to prisoner
            Vector3 endFlyPos = new Vector3(curPrisoner.gameObject.transform.position.x, (curPrisoner.gameObject.transform.position.y + 1f), curPrisoner.gameObject.transform.position.z);
            yield return StartCoroutine(HOTween.To(Julia.gameObject.transform, flyThroughSpeed, "position", endFlyPos).WaitForCompletion());

            //wait for julia out animation to finish
            yield return new WaitForSeconds(transitionJ(false));
            Julia.gameObject.SetActive(false);

            //wait for prisoner in animation to finish
            yield return new WaitForSeconds(transitionP(curPrisoner, true));

            //pan the camera out from pivot
            StartCoroutine(panFromHead(curPrisoner));

        //Prisoner to Prisoner
        } else if (!astralForm && (currentlyPossessing != null)) {
            canSwap = false;
            Julia.gameObject.transform.position = currentlyPossessing.transform.position;
            Vector3 turnDirection = curPrisoner.gameObject.transform.position - currentlyPossessing.gameObject.transform.position;
            turnDirection.Normalize();
            Julia.transform.GetComponent<SimpleMouseRotator>().enabled = false;

            //wait for prisoner out animation to finish
            yield return new WaitForSeconds(transitionP(currentlyPossessing, false));

            //Fly from prisoner to prisoner, 
            prisonerCamera.gameObject.SetActive(false);
            Julia.gameObject.SetActive(true);
            transitionJ(true);
            yield return StartCoroutine(HOTween.To(Julia.gameObject.transform, turnSpeed, "rotation", Quaternion.LookRotation(turnDirection)).WaitForCompletion());
            Vector3 endFlyPos = new Vector3(curPrisoner.gameObject.transform.position.x, (curPrisoner.gameObject.transform.position.y + 1f), curPrisoner.gameObject.transform.position.z);
            yield return StartCoroutine(HOTween.To(Julia.gameObject.transform, flyThroughSpeed, "position", endFlyPos).WaitForCompletion());
            prisonerCamera.gameObject.SetActive(true);
            transitionJ(false);
            Julia.gameObject.SetActive(false);

            //wait for in animation to finish
            yield return new WaitForSeconds(transitionP(curPrisoner, true));
            
            //pan the camera out from pivot
            StartCoroutine(panFromHead(curPrisoner));
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

            //make our camera be set so that it's always looking right behind the player
            prisonerCamera.gameObject.transform.rotation = Quaternion.LookRotation(curPrisoner.gameObject.transform.forward);
            prisonerCamera.LookAngle = prisonerCamera.gameObject.transform.eulerAngles.y;

            //set our current prisoners camera pivot positioning
            Vector3 pivotPosition = prisonerCamera.gameObject.transform.GetChild(0).localPosition;
            pivotPosition.y = curPrisoner.camPivVert;
            pivotPosition.x = curPrisoner.camPivHor;
            //set zoom and pivot
            prisonerCamera.gameObject.transform.GetChild(0).localPosition = pivotPosition;
            prisonerCamera.gameObject.transform.GetChild(0).GetChild(0).localPosition = new Vector3(0,0,0);

            //set our target and enable our animation
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

    private IEnumerator panFromHead(Prisoner curPrisoner) {
        Vector3 cameraZoom = prisonerCamera.gameObject.transform.GetChild(0).GetChild(0).localPosition;
        cameraZoom.z = curPrisoner.camZoom;
        yield return StartCoroutine( HOTween.To(prisonerCamera.gameObject.transform.GetChild(0).GetChild(0), 0.5f, "localPosition", cameraZoom).WaitForCompletion());
        curPrisoner.startControlling();
        canSwap = true;
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
            Julia.stopControlling();
            yield return new WaitForSeconds(transitionJ(true));
            Julia.startControlling();
        }
    }
}