using UnityEngine;
using System.Collections;

public class ControlManager : MonoBehaviour {

    public PossessionMaster possMaster;
    public Astral julia;
    private int curPosIndex;
    private bool guiEnabled;
	// Use this for initialization
	void Start () {
        possMaster = GameObject.Find("Possession Master").GetComponent<PossessionMaster>();
        julia = GameObject.Find("Julia").GetComponent<Astral>();
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp(KeyCode.E) && !PossessionMaster.AstralForm && possMaster.CanSwap) {
            StartCoroutine(possMaster.enterAstral());
        }
        if (Input.GetKeyUp(KeyCode.Q) && possMaster.CanSwap) {
            if (PossessionMaster.AstralForm && (possMaster.getInventory().Count >= 1)) {
                StartCoroutine(possMaster.swap(possMaster.getInventory()[0]));
            } else if(possMaster.getInventory().Count >1){
                ++curPosIndex;
                if (curPosIndex >= possMaster.getInventory().Count) {
                    curPosIndex = 0;
                    StartCoroutine(possMaster.swap(possMaster.getInventory()[curPosIndex]));
                } else {
                    StartCoroutine(possMaster.swap(possMaster.getInventory()[curPosIndex]));
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.R) && julia.CurrentlyViewing != null && !possMaster.getInventory().Contains(julia.CurrentlyViewing)) {
            julia.addPrisoner();
        }
	}
}
