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
        astralEffect = gameObject.GetComponent<AmplifyColorEffect>();
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
            //testing code
            Vector3 turnDirection =  curPrisoner.gameObject.transform.position - julia.gameObject.transform.position;
            turnDirection.Normalize();
            julia.stopControlling();
            yield return StartCoroutine(HOTween.To(julia.gameObject.transform, turnSpeed, "rotation", Quaternion.LookRotation(turnDirection)).WaitForCompletion());

            //Fly from current position to prisoner
            Vector3 endFlyPos = new Vector3(curPrisoner.gameObject.transform.position.x, (curPrisoner.gameObject.transform.position.y + 1f), curPrisoner.gameObject.transform.position.z);
            yield return StartCoroutine(HOTween.To(julia.gameObject.transform, flyThroughSpeed, "position", endFlyPos).WaitForCompletion());

            //wait for julia out animation to finish
            yield return new WaitForSeconds(transitionJ(false));
            julia.gameObject.SetActive(false);

            //wait for prisoner in animation to finish
            yield return new WaitForSeconds(transitionP(curPrisoner, true));

            //pan the camera out from pivot
            StartCoroutine(panHead(true, curPrisoner));

        //Prisoner to Prisoner
        } else if (!astralForm && (currentlyPossessing != null)) {
            canSwap = false;
            julia.gameObject.transform.position = currentlyPossessing.transform.position;
            //testing code
            Vector3 turnDirection = curPrisoner.gameObject.transform.position - currentlyPossessing.gameObject.transform.position;
            turnDirection.Normalize();
            julia.transform.GetComponent<SimpleMouseRotator>().enabled = false;

            //wait for prisoner out animation to finish
            yield return new WaitForSeconds(transitionP(currentlyPossessing, false));

            //Fly from prisoner to prisoner, 
            prisonerCamera.gameObject.SetActive(false);
            julia.gameObject.SetActive(true);
            transitionJ(true);
            yield return StartCoroutine(HOTween.To(julia.gameObject.transform, turnSpeed, "rotation", Quaternion.LookRotation(turnDirection)).WaitForCompletion());
            Vector3 endFlyPos = new Vector3(curPrisoner.gameObject.transform.position.x, (curPrisoner.gameObject.transform.position.y + 1f), curPrisoner.gameObject.transform.position.z);
            yield return StartCoroutine(HOTween.To(julia.gameObject.transform, flyThroughSpeed, "position", endFlyPos).WaitForCompletion());
            prisonerCamera.gameObject.SetActive(true);
            transitionJ(false);
            julia.gameObject.SetActive(false);

            //wait for in animation to finish
            yield return new WaitForSeconds(transitionP(curPrisoner, true));
            
            //pan the camera out from pivot
            StartCoroutine(panHead(true, curPrisoner));
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
        if (enter) {
            if (astralForm) {
                julia.gameObject.rigidbody.useGravity = false;
                julia.stopControlling();
            } else {
                currentlyPossessing.stopControlling();
                //pan the camera in
                yield return StartCoroutine(panHead(false, currentlyPossessing));

                //fade out astral effect
                HOTween.To(astralEffect, selectionSwapSettings.aeFadeSpeed, "BlendAmount", 0);

                //disable prisoner camera, enable julia camera
                prisonerCamera.gameObject.SetActive(false);
                julia.gameObject.transform.position = currentlyPossessing.gameObject.transform.GetChild(0).transform.position;
                julia.gameObject.SetActive(true);
                julia.gameObject.rigidbody.useGravity = false;
                julia.stopControlling();

            }

            Vector3 up = julia.gameObject.transform.position;
            up.y += selectionSwapSettings.popUp;
        
            //popUp
            yield return StartCoroutine(HOTween.To(julia.gameObject.transform, selectionSwapSettings.popUpSpeed, new TweenParms().Prop("position", up).Ease(EaseType.EaseOutQuad)).WaitForCompletion());

            //rotate to look at
            Vector3 turnDirection = highlighted.gameObject.transform.GetChild(0).position - julia.gameObject.transform.position;
            turnDirection.Normalize();
            HOTween.To(julia.gameObject.transform, selectionSwapSettings.rotateSpeed, "rotation", Quaternion.LookRotation(turnDirection));

            //zoom in on prisoner
            float distance = Vector3.Distance(PossessionMaster.AstralForm ? julia.gameObject.transform.position : PossessionMaster.CurrentlyPossesing.gameObject.transform.position, highlighted.gameObject.transform.position);
            HOTween.To(julia.gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Camera>(), selectionSwapSettings.zoomSpeed, "fieldOfView", origFovJulia - (distance * selectionSwapSettings.zoomMultiple));

        } else {
            //zoom back to normal
            yield return StartCoroutine(HOTween.To(julia.gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Camera>(), selectionSwapSettings.zoomSpeed, "fieldOfView", origFovJulia).WaitForCompletion());

            //fall back down
            Vector3 down = julia.gameObject.transform.position;
            down.y -= selectionSwapSettings.popUp;
            yield return StartCoroutine(HOTween.To(julia.gameObject.transform, selectionSwapSettings.popDownSpeed, new TweenParms().Prop("position", down).Ease(EaseType.EaseOutQuad)).WaitForCompletion());

            if (astralForm) {
                julia.gameObject.rigidbody.useGravity = true;
                julia.startControlling();
            } else {
                julia.gameObject.rigidbody.useGravity = true;
                julia.stopControlling();
                julia.gameObject.SetActive(false);
                transitionP(currentlyPossessing, true);

                //fade in astral effect
                HOTween.To(astralEffect, selectionSwapSettings.aeFadeSpeed, "BlendAmount", 1);

                //pan the camera out
                yield return StartCoroutine(panHead(true, currentlyPossessing));
                currentlyPossessing.startControlling();
            }

        }
        
    }

    //overloaded to account for if we are first starting selection, or stopping selection
    //public IEnumerator selectSwap(bool enter, Prisoner highlighted) {}

    //this function is used to panFrom the head when we arrive on a prisoner
    private IEnumerator panHead(bool entering, Prisoner curPrisoner) {
        Vector3 cameraZoom = prisonerCamera.gameObject.transform.GetChild(0).GetChild(0).localPosition;
        //this is used to set the vert look on prisoner entrance
        //prisonerCamera.gameObject.transform.GetChild(0).transform.eulerAngles = new Vector3(5, 0, 0);
        if (entering) {
            cameraZoom.z = curPrisoner.camZoom;
            yield return StartCoroutine(HOTween.To(prisonerCamera.gameObject.transform.GetChild(0).GetChild(0), panOutSpeed, "localPosition", cameraZoom).WaitForCompletion());
            curPrisoner.startControlling();
            canSwap = true;
        } else {
            yield return StartCoroutine(HOTween.To(prisonerCamera.gameObject.transform.GetChild(0).GetChild(0), panInSpeed, "localPosition", 0).WaitForCompletion());
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