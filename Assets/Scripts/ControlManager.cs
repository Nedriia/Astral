using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class ControlManager : MonoBehaviour {

    //public EasyButton enterAstral, swap, addPrisoner;
    public PossessionMaster possMaster;
    public Astral julia;
    public GameObject slideDoor;
    public GameObject endSlideDoor;
    public GuiManager gui;
    private int curPosIndex;
    private bool guiEnabled;
	// Use this for initialization
	void Start () {
        guiEnabled = gui.enabled;
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp(KeyCode.F)) {
            if (guiEnabled) {
                guiEnabled = false;
                gui.gameObject.SetActive(false);
            } else {
                guiEnabled = true;
                gui.gameObject.SetActive(true);
            }
        }
        if (Input.GetKeyUp(KeyCode.E) && !possMaster.AstralForm) {
            StartCoroutine(possMaster.enterAstral());
        }
        if (Input.GetKeyUp(KeyCode.Q) && (possMaster.getInventory().Count >= 1)) {
            if (possMaster.AstralForm) {
                StartCoroutine(possMaster.swap(possMaster.getInventory()[0]));
            } else {
                ++curPosIndex;
                if (curPosIndex > possMaster.getInventory().Count-1) {
                    curPosIndex = 0;
                    StartCoroutine(possMaster.swap(possMaster.getInventory()[curPosIndex]));
                } else {
                    StartCoroutine(possMaster.swap(possMaster.getInventory()[curPosIndex]));
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.R) && (julia.CurrentlyViewing != null) && (!possMaster.getInventory().Contains(julia.CurrentlyViewing))) {
            julia.addPrisoner();
        }

        if (Input.GetKeyUp(KeyCode.G)) {
            HOTween.To(slideDoor.transform, 1, "position", endSlideDoor.transform.position); 
        }
	}
}
