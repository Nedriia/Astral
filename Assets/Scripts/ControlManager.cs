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

        if (Input.GetKeyUp(KeyCode.E)) {
            StartCoroutine(possMaster.enterAstral());
        }
        if (Input.GetKeyUp(KeyCode.Q)) {
            if (PossessionMaster.AstralForm) {
                StartCoroutine(possMaster.swap(possMaster.getInventory()[0]));
            } else {
                ++curPosIndex;
                if (curPosIndex > possMaster.getInventory().Count) {
                    curPosIndex = 0;
                    StartCoroutine(possMaster.swap(possMaster.getInventory()[curPosIndex]));
                } else {
                    StartCoroutine(possMaster.swap(possMaster.getInventory()[curPosIndex]));
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.R)) {
            julia.addPrisoner();
        }
	}
}
