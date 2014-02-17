using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class ControlManager : MonoBehaviour {

    public PossessionMaster possMaster;
    public Astral julia;

    private int curPosIndex;
    private bool guiEnabled, selectionMode;
	// Use this for initialization
	void Start () {
        selectionMode = false;
        possMaster = GameObject.Find("Possession Master").GetComponent<PossessionMaster>();
        julia = GameObject.Find("Julia").GetComponent<Astral>();
	}
	
	// Update is called once per frame
	void Update () {
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

        if(Input.GetKeyUp(KeyCode.T)){
            julia.stopControlling();
            selectPris2Poss(possMaster.getInventory()[0]);
        }
        if(selectionMode){

        }
	}

    private void selectPris2Poss(Prisoner highlighted) {
        Vector3 turnDirection =  highlighted.gameObject.transform.position - julia.gameObject.transform.position;
        turnDirection.Normalize();
        HOTween.To(julia.gameObject.transform, 0.5f, "rotation", Quaternion.LookRotation(turnDirection));
        float fov = julia.gameObject.transform.GetChild(0).GetChild(0).GetComponent<Camera>().fieldOfView;
        HOTween.To(julia.gameObject.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Camera>(), 1.5f, "fieldOfView", fov -50);
    }
}
