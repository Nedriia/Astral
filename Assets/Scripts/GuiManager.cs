using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuiManager : MonoBehaviour {

    public EasyButton enterAstral, swap, addPrisoner;
    public PossessionMaster possMaster;
    public Astral julia;
    private int curPosIndex;
	// Use this for initialization
	void Start () {
        EasyButton.On_ButtonUp += On_ButtonUp;
        curPosIndex = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if (possMaster.AstralForm) {
            enterAstral.isActivated = false;
        } else {
            enterAstral.isActivated = true;
        }

        if ((possMaster.getInventory().Count == 0) || (!possMaster.AstralForm && possMaster.getInventory().Count == 1)) {
            swap.isActivated = false;
        } else {
            swap.isActivated = true;
        }

        if (julia.CurrentlyViewing != null && !possMaster.getInventory().Contains(julia.CurrentlyViewing)) {
            addPrisoner.isActivated = true;
        } else {
            addPrisoner.isActivated = false;
        }
	}

    void On_ButtonUp (string buttonName) {
        if (buttonName == "Enter Astral") {
            StartCoroutine(possMaster.enterAstral());
        }
        if (buttonName == "Swap") {
            if (possMaster.AstralForm) {
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
        if (buttonName == "Add Prisoner") {
            julia.addPrisoner();
        }
    }
}
