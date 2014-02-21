using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class ControlManager : MonoBehaviour {

    public PossessionMaster possMaster;
    public Astral julia;

    private int curPosIndex;
    private bool guiEnabled, selectionMode;
    // Use this for initialization
    void Start() {
        selectionMode = false;
        possMaster = GameObject.Find("Possession Master").GetComponent<PossessionMaster>();
        julia = GameObject.Find("Julia").GetComponent<Astral>();
    }

    // Update is called once per frame
    void Update() {
        //we make sure that we are not jumping 
        bool prisonerGrounded = (PossessionMaster.CurrentlyPossesing == null) ? false : PossessionMaster.CurrentlyPossesing.PrisonerAnimator.GetBool("OnGround");

        //Entering into astral
        if (Input.GetKeyUp(KeyCode.E) && !PossessionMaster.AstralForm && possMaster.CanSwap && prisonerGrounded) {
            StartCoroutine(possMaster.enterAstral());
        }

        //Swapping
        if (Input.GetKeyUp(KeyCode.Q) && possMaster.CanSwap) {
            if (PossessionMaster.AstralForm && (possMaster.getInventory().Count >= 1)) {
                StartCoroutine(possMaster.swap(possMaster.getInventory()[0]));
            } else if ((possMaster.getInventory().Count > 1) && prisonerGrounded) {
                ++curPosIndex;
                if (curPosIndex >= possMaster.getInventory().Count) {
                    curPosIndex = 0;
                    StartCoroutine(possMaster.swap(possMaster.getInventory()[curPosIndex]));
                } else {
                    StartCoroutine(possMaster.swap(possMaster.getInventory()[curPosIndex]));
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.R) && julia.CurrentlyViewing != null && !possMaster.getInventory().Contains(julia.CurrentlyViewing) && !julia.CurrentlyViewing.IsDead) {
            julia.addPrisoner();
        }

        if (Input.GetKeyUp(KeyCode.G)) {
            if (!selectionMode) {
                StartCoroutine(possMaster.selectSwap(true, possMaster.getInventory()[0]));
                selectionMode = true;
            } else {
                StartCoroutine(possMaster.selectSwap(false, possMaster.getInventory()[0]));
                selectionMode = false;
            }
        }
        if (Input.GetKeyUp(KeyCode.T)) {
            if (selectionMode) {
                julia.gameObject.rigidbody.useGravity = true;
                selectionMode = false;
                HOTween.To(julia.gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Camera>(), 0.5f, "fieldOfView", 60);
                julia.startControlling();
            } else {
                julia.gameObject.rigidbody.useGravity = false;
                selectionMode = true;
                StartCoroutine(astralUp());
                //Vector3 up = julia.gameObject.transform.position;
                //up.y += 1.5f;
                // HOTween.To(julia.gameObject.transform, 0.5f, new TweenParms().Prop("position", up).Ease(EaseType.EaseOutQuad));
                // curPosIndex = 0;
                //julia.stopControlling();
                // selectPris2Poss(possMaster.getInventory()[0]);
            }
        }
        if (selectionMode) {
            if (Input.GetKeyUp(KeyCode.LeftArrow)) {
                StartCoroutine(deselectPris2Pos(true));
            }
            if (Input.GetKeyUp(KeyCode.RightArrow)) {
                StartCoroutine(deselectPris2Pos(false));
            }
        }
    }

    private void selectPris2Poss(Prisoner highlighted) {
        Vector3 highlightedUp = highlighted.gameObject.transform.position;
        highlightedUp.y += 1.8f;
        Vector3 turnDirection = highlightedUp - julia.gameObject.transform.position;
        turnDirection.Normalize();
        HOTween.To(julia.gameObject.transform, 0.5f, "rotation", Quaternion.LookRotation(turnDirection));
        float fov = julia.gameObject.transform.GetChild(0).GetChild(0).GetComponent<Camera>().fieldOfView;
        float distance = Vector3.Distance(PossessionMaster.AstralForm ? julia.gameObject.transform.position : PossessionMaster.CurrentlyPossesing.gameObject.transform.position, highlighted.gameObject.transform.position);
        Debug.Log("Distance " + distance + "Distance %" + distance * 0.50f);
        HOTween.To(julia.gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Camera>(), 0.5f, "fieldOfView", fov - (distance * 2));
    }
    private IEnumerator deselectPris2Pos(bool left) {
        yield return StartCoroutine(HOTween.To(julia.gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Camera>(), 0.5f, "fieldOfView", 60).WaitForCompletion());
        if (left) {
            selectPris2Poss(possMaster.getInventory()[(curPosIndex == 0) ? possMaster.getInventory().Count - 1 : --curPosIndex]);
            if (curPosIndex == 0)
                curPosIndex = possMaster.getInventory().Count - 1;
        } else {
            selectPris2Poss(possMaster.getInventory()[(curPosIndex == possMaster.getInventory().Count - 1) ? 0 : ++curPosIndex]);
            if (curPosIndex == (possMaster.getInventory().Count - 1))
                curPosIndex = 0;
        }
    }

    private IEnumerator astralUp() {
        Vector3 up = julia.gameObject.transform.position;
        up.y += 1.5f;
        yield return StartCoroutine(HOTween.To(julia.gameObject.transform, 0.5f, new TweenParms().Prop("position", up).Ease(EaseType.EaseOutQuad)).WaitForCompletion());
        curPosIndex = 0;
        julia.stopControlling();
        selectPris2Poss(possMaster.getInventory()[0]);
    }
}
