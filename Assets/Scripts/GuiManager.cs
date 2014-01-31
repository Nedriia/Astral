using UnityEngine;
using System.Collections;

public class GuiManager : MonoBehaviour {

    public EasyButton enterAstral;
    public PossessionMaster possMaster;
	// Use this for initialization
	void Start () {
        EasyButton.On_ButtonUp += On_ButtonUp;	
	}
	
	// Update is called once per frame
	void Update () {
        if (possMaster.AstralForm) {
            enterAstral.isActivated = false;
        } else {
            enterAstral.isActivated = true;
        }
	}

    void On_ButtonUp (string buttonName) {
        if (buttonName == "Enter Astral") {
            StartCoroutine(possMaster.enterAstral());
        }
    }
}
