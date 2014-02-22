using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;

public class PossessionMaster : MonoBehaviour {

    public Astral julia;
    public FreeLookCam prisonerCamera;
    public Prisoner startingPosPrisoner;
    public float flyThroughSpeed;                              //how fast to the possessing prisoner
    public float turnSpeed;                                    //how fast we turn to look at the prisoner
    public float panOutSpeed = 0.5f;                           //how fast we pan from the head of the prisoner we just entered
    public float panInSpeed = 0.5f;                            //how fast we pan in from the head of the prisoner we are possessing
    public SelectionSwapSettings selectionSwapSettings;

    [System.Serializable]
    public class SelectionSwapSettings{
        public float popUp = 1.5f;
        public float popUpSpeed = 0.5f;
        public float popDownSpeed = 0.5f;
        public float aeFadeSpeed = 1.5f;
        public float rotateSpeed = 0.5f;
        public float zoomMultiple = 2f;
        public float zoomSpeed = 0.5f;

    }
    private List<Prisoner> prisonerInventory;
    private static Prisoner currentlyPossessing;
    private static bool astralForm;
    private bool canSwap;
    private AmplifyColorEffect astralEffect;
    private float origFovJulia;

	// Use this for initialization
	void Start () {
        julia = GameObject.Find("Julia").GetComponent<Astral>();
        origFovJulia = julia.gameObject.transform.GetChild(0).GetChild(0).GetComponent<Camera>().fieldOfView;
        prisonerCamera = GameObject.Find("Prisoner Camera Rig").GetComponent<FreeLookCam>();
        astralEffect = julia.gameObject.transform.GetChild(0).GetChild(0).GetComponent<AmplifyColorEffect>();
        prisonerInventory = new List<Prisoner>();
        canSwap = true;
        //we are starting in a prisoner
        if (startingPosPrisoner != null) {
            StartCoroutine(possesionStart(true));
        //we are staring in julia
        } else {
            StartCoroutine(possesionStart(false));
        }
	}
	
	// Update is called once per frame
	void Update () {

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
        //Prisoner to julia
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
        julia.gameObject.SetActive(true);
        julia.transform.position = juliaNewPosition; 
        yield return new WaitForSeconds(transitionJ(true));
        julia.startControlling();
        canSwap = true;
    }

    //swaps to the specified prisoner
    public IEnumerator swap( Prisoner curPrisoner ) {
        //julia to prisoner
        if ( astralForm && (currentlyPossessing == null)) {
            canSwap = false;

            //make sure that she can't move any longer
            julia.stopControlling();

            yield return StartCoroutine(astralTurnTowardPivot(curPrisoner, true));

            //Fly from current position to prisoner
            yield return StartCoroutine(HOTween.To(julia.gameObject.transform, flyThroughSpeed, "position", curPrisoner.focusPoint.position).WaitForCompletion());

            //wait for julia out animation to finish
            yield return new WaitForSeconds(transitionJ(false));
            julia.gameObject.SetActive(false);

            //wait for prisoner in animation to finish
            yield return new WaitForSeconds(transitionP(curPrisoner, true));

            //pan the camera out from pivot
            yield return StartCoroutine(panHead(true, curPrisoner));
            curPrisoner.startControlling();
            canSwap = true;

        //Prisoner to Prisoner
        } else if (!astralForm && (currentlyPossessing != null)) {
            canSwap = false;
            julia.gameObject.transform.position = currentlyPossessing.transform.position;
            julia.transform.GetComponent<SimpleMouseRotator>().enabled = false;

            //wait for prisoner out animation to finish
            yield return new WaitForSeconds(transitionP(currentlyPossessing, false));

            //Fly from prisoner to prisoner, 
            prisonerCamera.gameObject.SetActive(false);
            julia.gameObject.SetActive(true);
            transitionJ(true);
            StartCoroutine(astralTurnTowardPivot(curPrisoner, false));
            yield return StartCoroutine(HOTween.To(julia.gameObject.transform, flyThroughSpeed, "position", curPrisoner.gameObject.transform.GetChild(0).position).WaitForCompletion());
            prisonerCamera.gameObject.SetActive(true);
            transitionJ(false);
            julia.gameObject.SetActive(false);

            //wait for in animation to finish
            yield return new WaitForSeconds(transitionP(curPrisoner, true));
            
            //pan the camera out from pivot
            yield return StartCoroutine(panHead(true, curPrisoner));
            curPrisoner.startControlling();
            canSwap = true;
        }
    }


    //transtions in or out of julia
    private float transitionJ(bool enter) {
        float waitTime = 0;
        if (enter) {
            waitTime = julia.bodyTransition(true);
            astralForm = true;
        } else {
            julia.stopControlling();
            waitTime = julia.bodyTransition(false);
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

    public IEnumerator selectSwap(bool enter, Prisoner highlighted) {
        canSwap = false;
        if (enter) {
            if (astralForm) {
                julia.stopControlling();
                julia.gameObject.rigidbody.isKinematic = true;
            } else {
                currentlyPossessing.stopControlling();
                //pan the camera in
                yield return StartCoroutine(panHead(false, currentlyPossessing));

                //fade out astral effect
                astralEffect.BlendAmount = 0;
                HOTween.To(astralEffect, selectionSwapSettings.aeFadeSpeed, "BlendAmount", 0.7f);

                //disable prisoner camera, enable julia camera
                prisonerCamera.gameObject.SetActive(false);
                julia.gameObject.transform.position = currentlyPossessing.gameObject.transform.GetChild(0).transform.position;
                julia.gameObject.SetActive(true);

                julia.stopControlling();
                julia.gameObject.rigidbody.isKinematic = true;

            }
            if (astralForm) {
                StartCoroutine(astralTurnTowardPivot(highlighted, true));
            } else {
                StartCoroutine(astralTurnTowardPivot(highlighted, false));
            }

            Vector3 upAir = julia.gameObject.transform.position;
            upAir.y += selectionSwapSettings.popUp;

            //popUp
            yield return StartCoroutine(HOTween.To(julia.gameObject.transform, selectionSwapSettings.popUpSpeed, new TweenParms().Prop("position", upAir).Ease(EaseType.EaseOutQuad)).WaitForCompletion());

            //zoom in on prisoner
            float distance = Vector3.Distance(PossessionMaster.AstralForm ? julia.gameObject.transform.position : PossessionMaster.CurrentlyPossesing.gameObject.transform.position, highlighted.gameObject.transform.position);
            yield return StartCoroutine(HOTween.To(julia.gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Camera>(), selectionSwapSettings.zoomSpeed, "fieldOfView", origFovJulia - (distance * selectionSwapSettings.zoomMultiple)).WaitForCompletion());
            
        } else {
            //zoom back to normal
            yield return StartCoroutine(HOTween.To(julia.gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Camera>(), selectionSwapSettings.zoomSpeed, "fieldOfView", origFovJulia).WaitForCompletion());

            if(!astralForm)
                //fade in astral effect
                HOTween.To(astralEffect, selectionSwapSettings.aeFadeSpeed, "BlendAmount", 0);

            //fall back down
            Vector3 down = julia.gameObject.transform.position;
            down.y -= selectionSwapSettings.popUp;
            yield return StartCoroutine(HOTween.To(julia.gameObject.transform, selectionSwapSettings.popDownSpeed, new TweenParms().Prop("position", down).Ease(EaseType.EaseOutQuad)).WaitForCompletion());

            if (astralForm) {
                julia.gameObject.rigidbody.isKinematic = false;
                julia.startControlling();
            } else {
                julia.gameObject.rigidbody.isKinematic = false;
                julia.stopControlling();
                julia.gameObject.SetActive(false);
                transitionP(currentlyPossessing, true);

                //pan the camera in
                yield return StartCoroutine(panHead(true, currentlyPossessing));
                currentlyPossessing.startControlling();
            }

        }
        canSwap = true;
    }

    //overloaded to account for if we are first starting selection, or stopping selection
    //public IEnumerator selectSwap(bool enter, Prisoner highlighted) {}

    //this function is used to panFrom the head when we arrive on a prisoner
    private IEnumerator panHead(bool panOut, Prisoner curPrisoner) {
        Vector3 cameraZoom = prisonerCamera.gameObject.transform.GetChild(0).GetChild(0).localPosition;
        //this is used to set the vert look on prisoner entrance
        //prisonerCamera.gameObject.transform.GetChild(0).transform.eulerAngles = new Vector3(5, 0, 0);
        if (panOut) {
            cameraZoom.z = curPrisoner.camZoom;
            yield return StartCoroutine(HOTween.To(prisonerCamera.gameObject.transform.GetChild(0).GetChild(0), panOutSpeed, "localPosition", cameraZoom).WaitForCompletion());
        } else {
            cameraZoom.z = 0;
            yield return StartCoroutine(HOTween.To(prisonerCamera.gameObject.transform.GetChild(0).GetChild(0), panInSpeed, "localPosition", cameraZoom ).WaitForCompletion());
        }
    }

    private IEnumerator astralTurnTowardPivot(Prisoner prisoner, bool tween) {
        Vector3 horizontalShift = julia.gameObject.transform.position, verticalShift = julia.gameObject.transform.GetChild(0).GetChild(0).position;

        horizontalShift.y = prisoner.gameObject.transform.GetChild(0).position.y;
        verticalShift.y += selectionSwapSettings.popUp;

        //rotate to look at
        Vector3 horizontalTurn = prisoner.focusPoint.position - horizontalShift, verticalTurn = prisoner.focusPoint.position - verticalShift;

        Debug.Log(prisoner.gameObject.transform.GetChild(0).name);
        horizontalTurn.Normalize();
        verticalTurn.Normalize();

        if (tween) {
            HOTween.To(julia.gameObject.transform.GetChild(0).GetChild(0), selectionSwapSettings.rotateSpeed, "rotation", Quaternion.LookRotation(verticalTurn));
            yield return StartCoroutine(HOTween.To(julia.gameObject.transform, selectionSwapSettings.rotateSpeed, "rotation", Quaternion.LookRotation(horizontalTurn)).WaitForCompletion());
        } else {
            julia.gameObject.transform.rotation = Quaternion.LookRotation(horizontalTurn);
            julia.gameObject.transform.GetChild(0).GetChild(0).transform.rotation = Quaternion.LookRotation(verticalTurn);
        }
    }

    //We call this on the start of our game
    private IEnumerator possesionStart(bool startAsPrisoner) {
        if (startAsPrisoner) {
            julia.gameObject.SetActive(false);
            astralForm = false;
            yield return new WaitForSeconds(transitionP(startingPosPrisoner, true));
            currentlyPossessing.startControlling();
        } else {
            astralForm = true;
            prisonerCamera.gameObject.SetActive(false);
            julia.stopControlling();
            yield return new WaitForSeconds(transitionJ(true));
            julia.startControlling();
        }
    }
}